using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private CustomVirtualFile EnsureFileHasEntry(string virtualPath)
        {
            var path = NormalizeFilePath(virtualPath);

            var cacheKey = GetCacheKeyForFile(path);

            if (Cache.HasEntry(cacheKey)) return null;

            var info = _service.GetFileInfo(path);
            if (info.Item1)
            {
                var vf = new CustomVirtualFile(virtualPath, info.Item2, info.Item3);
                Cache.Set(GetCacheKeyForHash(path), vf.Hash, 2, false);
                Cache.Set(cacheKey, vf);
                return vf;
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

        //public override bool IsVirtualDir(string virtualPath)
        //{
        //    return false;
        //}


        //todo: eager load all files in directory
        public override bool DirectoryExists(string virtualDir)
        {
            return false;

            //var path = NormalizeFilePath(virtualDir);

            //var cacheKey = GetCacheKeyForDir(path);
            //var item = Cache.Get(cacheKey);
            //if (item != null)
            //{
            //    return item is CustomVirtualDir;
            //}

            //var result = _service.DirectoryExistsImpl(path);
            //if (result)
            //{
            //    GetDirectoryInternal(virtualDir);
            //}
            //else
            //{
            //    Cache.Set(cacheKey, new DummyVirtualFile(true), 20, false);
            //}

            //return result;
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

        private static string GetCacheKeyForFile(string path)
        {
            //f = file
            return string.Format("F{0}", path);
        }

        private static string GetCacheKeyForHash(string path)
        {
            //f = file
            return string.Format("H{0}", path);
        }

        public override void RemoveFromCache(string virtualPath)
        {
            var path = NormalizeFilePath(virtualPath);
            Trace.TraceInformation("RemoveFromCache: {0}", virtualPath);

            var key = GetCacheKeyForFile(path);
            Cache.Remove(key);
        }

        public override CustomVirtualDir GetDirectory(string virtualDir)
        {
            return GetDirectoryInternal(virtualDir);
        }

        private CustomVirtualDir GetDirectoryInternal(string virtualDir)
        {
            return null;

            //var path = NormalizeFilePath(virtualDir);

            //var cacheKey = GetCacheKeyForDir(path);

            //var item = Cache.Get(cacheKey);
            //if (item != null)
            //{
            //    Trace.TraceInformation("From cache: {0} - '{1}'", item, cacheKey);
            //    return (CustomVirtualDir)item;
            //}
            //var id = _service.GetDirectoryId(path);

            //var dir = new CustomVirtualDir(virtualDir, () => LazyGetChildren(id));


            //Cache.Set(cacheKey, dir);

            //return dir;
        }
    }
}