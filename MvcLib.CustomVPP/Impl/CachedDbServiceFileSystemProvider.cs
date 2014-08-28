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

        public override bool FileExists(string virtualPath)
        {
            var path = NormalizeFilePath(virtualPath);

            var cacheKey = GetCacheKeyForFile(path);
            var item = Cache.Get(cacheKey);
            if (item != null)
            {
                return item is CustomVirtualFile || item is ICacheableBytes;
            }

            var result = _service.FileExistsImpl(path);

            if (result)
                Cache.Set(cacheKey, GetFileInternal(virtualPath, true));
            else
            {
                Cache.Set(cacheKey, new DummyVirtualFile(false), 20, false);
            }

            return result;

        }

        //public override bool IsVirtualDir(string virtualPath)
        //{
        //    return false;
        //}


        //todo: eager load all files in directory
        public override bool DirectoryExists(string virtualDir)
        {
            // return false;

            var path = NormalizeFilePath(virtualDir);

            var cacheKey = GetCacheKeyForDir(path);
            var item = Cache.Get(cacheKey);
            if (item != null)
            {
                return item is CustomVirtualDir;
            }

            var result = _service.DirectoryExistsImpl(path);
            if (result)
            {
                Cache.Set(cacheKey, GetDirectoryInternal(virtualDir, true));
            }
            else
            {
                Cache.Set(cacheKey, new DummyVirtualFile(true), 20, false);
            }

            return result;
        }

        public override CustomVirtualFile GetFile(string virtualPath)
        {
            return GetFileInternal(virtualPath, false);
        }

        public override CustomVirtualDir GetDirectory(string virtualDir)
        {
            return GetDirectoryInternal(virtualDir, false);
        }

        public override string GetFileHash(string virtualPath)
        {
            var path = NormalizeFilePath(virtualPath);
            var cacheKey = GetCacheKeyForHash(path);

            string hash = (string)Cache.Get(cacheKey);
            if (!string.IsNullOrWhiteSpace( hash))
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
                RemoveFromCache(path, true, true, true);
            }
            else
            {
                var fileKey = GetCacheKeyForFile(path);
                var vf = Cache.Get(fileKey) as CustomVirtualFile;
                if (vf == null || vf.Hash != hash)
                {
                    //arquivo foi alterado
                    RemoveFromCache(path, true, true, false);
                }
            }
            return hash;
        }

        public override IEnumerable<VirtualFileBase> LazyGetChildren(int key)
        {
            foreach (var dbFile in _service.GetChildren(key))
            {
                var current = dbFile;
                if (current.Item2)
                {
                    yield return GetDirectoryInternal(current.Item1, false);
                }
                else
                {
                    yield return GetFileInternal(current.Item1, false);
                }
            }
        }

        public override void RemoveFromCache(string key)
        {
            var path = NormalizeFilePath(key);
            RemoveFromCache(path, false, true, true);
        }

        private CustomVirtualFile GetFileInternal(string virtualPath, bool isSettingCache)
        {
            var path = NormalizeFilePath(virtualPath);

            var cacheKey = GetCacheKeyForFile(path);

            object item = Cache.Get(cacheKey);
            if (item != null)
            {
                Trace.TraceInformation("From cache: {0} - '{1}'", item, cacheKey);
                return (CustomVirtualFile)item;
            }

            var bytes = _service.GetFileBytes(path);    
            var hash = GetFileHash(virtualPath);

            var file = new CustomVirtualFile(virtualPath, bytes, hash);

            if (!isSettingCache)
                Cache.Set(cacheKey, file);

            return file;
        }

        private CustomVirtualDir GetDirectoryInternal(string virtualDir, bool isSettingCache)
        {
            var path = NormalizeFilePath(virtualDir);

            var cacheKey = GetCacheKeyForDir(path);

            var item = Cache.Get(cacheKey);
            if (item != null)
            {
                Trace.TraceInformation("From cache: {0} - '{1}'", item, cacheKey);
                return (CustomVirtualDir)item;
            }
            var id = _service.GetDirectoryId(path);

            var dir = new CustomVirtualDir(virtualDir, () => LazyGetChildren(id));

            if (!isSettingCache)
                Cache.Set(cacheKey, dir);

            return dir;

        }

        private static string GetCacheKeyForFile(string path)
        {
            //f = file
            return string.Format("F{0}", path);
        }

        private static string GetCacheKeyForDir(string path)
        {
            //d = dir
            return string.Format("D{0}", path);
        }

        private static string GetCacheKeyForHash(string path)
        {
            //h = hash
            return string.Format("H{0}", path);
        }

        internal void RemoveFromCache(string path, bool removeDir, bool removeFile, bool removeHash)
        {
            Trace.TraceInformation("RemoveFromCache: {0}", path);

            if (removeDir)
            {
                var key = GetCacheKeyForDir(path);
                Cache.Remove(key);
            }
            if (removeFile)
            {
                var key = GetCacheKeyForFile(path);
                Cache.Remove(key);
            }

            if (removeHash)
            {
                var hash = GetCacheKeyForHash(path);
                Cache.Remove(hash);
            }
        }
    }
}