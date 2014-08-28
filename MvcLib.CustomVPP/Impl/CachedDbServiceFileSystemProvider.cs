using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using MvcLib.Common.Cache;

namespace MvcLib.CustomVPP.Impl
{
    public class CachedDbServiceFileSystemProvider : AbstractFileSystemProvider
    {
        private readonly IDbService _service;
        public ICacheProvider Cache { get; private set; }

        /*
         * Usar ~/bundles/ para CSS e JS
         * Caso contrário, utilizar StaticFileHandler no web.config
         */

        public CachedDbServiceFileSystemProvider(IDbService service, ICacheProvider cache)
        {
            _service = service;
            Cache = cache;
        }

        public override void Initialize()
        {
            base.Initialize();

            //pre carregar diretorios e arquivos
            var files = _service.Preload();
        }

        #region files

        private CustomVirtualFile SetEntry(string virtualPath, string hash, byte[] bytes)
        {
            var path = NormalizeFilePath(virtualPath);
            var cacheKey = GetCacheKeyForFile(path);

            var vf = new CustomVirtualFile(virtualPath, hash, bytes);
            Cache.Set(GetCacheKeyForHash(path), vf.Hash, 2, false);
            Cache.Set(cacheKey, vf);
            return vf;
        }

        private CustomVirtualFile EnsureFileHasEntry(string virtualPath)
        {
            var path = NormalizeFilePath(virtualPath);

            var cacheKey = GetCacheKeyForFile(path);

            if (Cache.HasEntry(cacheKey)) return null;

            var info = _service.GetFileInfo(path);
            if (info.Item1)
            {
                return SetEntry(virtualPath, info.Item2, info.Item3);
            }

            Cache.Set(GetCacheKeyForHash(path), info.Item2, 2, false);
            Cache.Set(cacheKey, (string)null);

            return null;
        }

        public override bool FileExists(string virtualPath)
        {
            var entry = EnsureFileHasEntry(virtualPath);
            if (entry != null)
            {
                return true;
            }

            //arquivo já está no cache (como existente ou não)

            var path = NormalizeFilePath(virtualPath);
            var cacheKey = GetCacheKeyForFile(path);

            var item = Cache.Get(cacheKey);
            if (item != null)
            {
                //arquivo existe se for do tipo correto
                return item is CustomVirtualFile;
            }

            return false;
        }

        public override CustomVirtualFile GetFile(string virtualPath)
        {
            var info = EnsureFileHasEntry(virtualPath);

            if (info != null)
            {
                return info;
            }

            var path = NormalizeFilePath(virtualPath);

            var cacheKey = GetCacheKeyForFile(path);

            CustomVirtualFile item = Cache.Get(cacheKey) as CustomVirtualFile;
            if (item != null)
            {
                Trace.TraceInformation("From cache: {0} - '{1}'", item, cacheKey);
                return item;
            }

            return null;
        }

        public override string GetFileHash(string virtualPath)
        {
            var info = EnsureFileHasEntry(virtualPath);

            if (info != null)
            {
                return info.Hash;
            }

            var path = NormalizeFilePath(virtualPath);
            var cacheKey = GetCacheKeyForHash(path);

            string hash = (string)Cache.Get(cacheKey);
            if (!string.IsNullOrWhiteSpace(hash))
            {
                return hash;
            }

            hash = _service.GetFileHash(path);
            //hash: 2 minutos sem sliding
            //verificar se arquivo foi alterado ou excluído
            Cache.Set(cacheKey, hash, 2, false);

            if (string.IsNullOrWhiteSpace(hash))
            {
                //não encontrou hash no banco: arquivo foi excluído
                Trace.TraceInformation("File was deleted? {0}", path);
                RemoveFromCache(path);
            }
            else
            {
                var fileKey = GetCacheKeyForFile(path);
                var vf = Cache.Get(fileKey) as CustomVirtualFile;
                if (vf == null || vf.Hash != hash)
                {
                    //arquivo foi alterado
                    Trace.TraceInformation("File was modified? {0}", path);
                    RemoveFromCache(virtualPath);
                }
            }
            return hash;
        }

        #endregion

        #region directory

        public override bool DirectoryExists(string virtualDir)
        {
            //return false;

            var path = NormalizeFilePath(virtualDir);

            var cacheKey = GetCacheKeyDir(path);
            var item = Cache.Get(cacheKey);
            if (item != null)
            {
                return item is CustomVirtualDir;
            }

            var result = _service.DirectoryExistsImpl(path);

            // a chave será sobrescrita quando for recuperar o diretório real
            Cache.Set(cacheKey, new CustomVirtualDir(virtualDir, false, Enumerable.Empty<VirtualFileBase>()));

            return result;
        }

        //todo: eager load all files in directory

        public override CustomVirtualDir GetDirectory(string virtualDir)
        {
            //return null;

            var path = NormalizeFilePath(virtualDir);

            var cacheKey = GetCacheKeyDir(path);

            var item = Cache.Get(cacheKey) as CustomVirtualDir;
            if (item != null && item.Loaded)
                return item;

            //load files and subdirectories

            var children = new List<VirtualFileBase>();

            var tuples = _service.GetChildren(path);
            foreach (var tuple in tuples)
            {
                var dirKey = GetCacheKeyDir(tuple.Item1);
                var vpp = "~" + tuple.Item1;
                if (string.IsNullOrWhiteSpace(tuple.Item2))
                {
                    //sem hash = diretorio
                    if (Cache.HasEntry(dirKey))
                    {
                        var entry = Cache.Get(dirKey) as CustomVirtualDir;
                        if (entry != null && entry.Loaded)
                            continue;
                    }

                    var dir = new CustomVirtualDir(vpp, false, Enumerable.Empty<VirtualFileBase>());
                    Cache.Set(dirKey, dir);
                    children.Add(dir);
                }
                else
                {
                    //guarda arquivo no cache
                    var entry = SetEntry(vpp, tuple.Item2, tuple.Item3);
                    children.Add(entry);
                }
            }
            var result = new CustomVirtualDir(virtualDir, true, children);
            Cache.Set(cacheKey, result);
            return result;
        }

        #endregion

        public override void RemoveFromCache(string virtualPath)
        {
            var path = NormalizeFilePath(virtualPath);
            Trace.TraceInformation("RemoveFromCache: {0}", virtualPath);

            var fkey = GetCacheKeyForFile(path);
            Cache.Remove(fkey);

            var hkey = GetCacheKeyForHash(path);
            Cache.Remove(hkey);

            var dkey = GetCacheKeyDir(path);
            Cache.Remove(dkey);

        }

        #region helpers

        private static string GetCacheKeyForFile(string path)
        {
            //f = file
            return string.Format("F{0}", path);
        }

        private static string GetCacheKeyForHash(string path)
        {
            //h = hash
            return string.Format("H{0}", path);
        }

        private static string GetCacheKeyDir(string path)
        {
            //d = dir
            return string.Format("D{0}", path);
        }

        #endregion
    }
}