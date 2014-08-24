using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web;
using System.Web.Hosting;
using MvcFromDb.Infra.Misc;

namespace MvcFromDb.Infra.VPP.Impl
{
    public class CachedDbServiceFileSystemProvider : AbstractFileSystemProvider
    {
        private readonly IDbService _service;
        private static readonly string CacheKeySalt = new Random().Next(0, 999).ToString("d3");

        /*
         * Usar ~/bundles/ para CSS e JS
         * Caso contrário, utilizar StaticFileHandler no web.config
         */

        public CachedDbServiceFileSystemProvider(IDbService service)
        {
            _service = service;
        }

        public override bool FileExists(string virtualPath)
        {
            var path = NormalizeFilePath(virtualPath);

            var cacheKey = GetCacheKeyForFile(path);
            var item = CacheWrapper.Get(cacheKey);
            if (item != null)
            {
                return item is CustomVirtualFile;
            }

            var result = _service.FileExistsImpl(path);

            if (result)
                CacheWrapper.Set(cacheKey, GetFileInternal(virtualPath, true));
            else
            {
                CacheWrapper.Set(cacheKey, new DummyVirtualFile(false), 20, false);
            }

            return result;

        }

        public override bool IsVirtualDir(string virtualPath)
        {
            return false;
        }

        public override bool DirectoryExists(string virtualDir)
        {
            return false;

            //var path = NormalizeFilePath(virtualDir);

            //var cacheKey = GetCacheKeyForDir(path);
            //var item = CacheWrapper.Get(cacheKey);
            //if (item != null)
            //{
            //    return item is CustomVirtualDir;
            //}

            //var result = _service.DirectoryExistsImpl(path);
            //if (result)
            //{
            //    CacheWrapper.Set(cacheKey, GetDirectoryInternal(virtualDir, true));
            //}
            //else
            //{
            //    CacheWrapper.Set(cacheKey, new DummyVirtualFile(true), 20, false);
            //}

            //return result;
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

            string item = CacheWrapper.Get(cacheKey);
            if (item != null)
            {
                return item;
            }

            item = _service.GetFileHash(path);

            var vf = CacheWrapper.Get(GetCacheKeyForFile(path)) as CustomVirtualFile;
            if (vf == null || vf.Hash != item)
            {
                RemoveFileFromCache(path, false);
            }

            return CacheWrapper.Set(cacheKey, item, 2, false);
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

        private CustomVirtualFile GetFileInternal(string virtualPath, bool isSettingCache)
        {
            var path = NormalizeFilePath(virtualPath);

            var cacheKey = GetCacheKeyForFile(path);

            var item = CacheWrapper.Get(cacheKey);
            if (item != null)
            {
                return item as CustomVirtualFile;
            }

            var bytes = _service.GetFileBytes(path);
            var hash = _service.GetFileHash(path);

            item = new CustomVirtualFile(virtualPath, bytes, hash);
            if (!isSettingCache)
                CacheWrapper.Set(cacheKey, item);

            return item;
        }

        private CustomVirtualDir GetDirectoryInternal(string virtualDir, bool isSettingCache)
        {
            var path = NormalizeFilePath(virtualDir);

            var cacheKey = GetCacheKeyForDir(path);

            var item = CacheWrapper.Get(cacheKey);
            if (item != null)
            {
                return (CustomVirtualDir)item;
            }
            var id = _service.GetDirectoryId(path);

            item = new CustomVirtualDir(virtualDir, () => LazyGetChildren(id));

            if (!isSettingCache)
                CacheWrapper.Set(cacheKey, item);

            return item;

        }

        private static string GetCacheKeyForFile(string path)
        {
            //f = file
            return string.Format("F{0}:{1}", CacheKeySalt, path);
        }

        private static string GetCacheKeyForDir(string path)
        {
            //d = dir
            return string.Format("D{0}:{1}", CacheKeySalt, path);
        }

        private static string GetCacheKeyForHash(string path)
        {
            //h = hash
            return string.Format("H{0}:{1}", CacheKeySalt, path);
        }

        public static void RemoveFileFromCache(string path, bool isDir)
        {
            Trace.TraceInformation("RemoveFileFromCache: {0}", path);

            if (isDir)
            {
                var key = GetCacheKeyForDir(path);
                CacheWrapper.Remove(key);
            }
            else
            {
                var key = GetCacheKeyForFile(path);
                CacheWrapper.Remove(key);

                var hash = GetCacheKeyForHash(path);
                CacheWrapper.Remove(hash);
            }
        }

        private static string NormalizeFilePath(string virtualPath)
        {
            var absolute = VirtualPathUtility.ToAbsolute(virtualPath);

            var result = VirtualPathUtility.RemoveTrailingSlash(absolute);

            return result.ToLowerInvariant();
        }

    }
}