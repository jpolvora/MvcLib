using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using MvcFromDb.Infra.Misc;

namespace MvcFromDb.Infra.VPP.Impl
{
    public class DbVirtualPathProvider : IVirtualPathProvider
    {
        private readonly IDbService _service;
        private static readonly string CacheKeySalt = new Random().Next(0, 999).ToString("d3");

        /*
         * Usar ~/bundles/ para CSS e JS
         * Caso contrário, utilizar StaticFileHandler no web.config
         */
        private readonly string[] _allowedExtensions = { ".cshtml", ".js", ".css", ".xml", ".config" };
        private readonly string[] _ignoredFiles = { "precompiledapp.config" };
        private readonly string[] _ignoredDirectories = { "/bundles", "/app_localresources", "/app_browsers" };

        public DbVirtualPathProvider(IDbService service)
        {
            _service = service;
        }

        public void Initialize()
        {
            Trace.TraceInformation("DbVirtualPathProvider Initialized with CacheKey generated: {0}", CacheKeySalt);
        }

        public bool IsVirtualDir(string virtualDir)
        {
            /*
            * Se o path possuir extension, ignora.            
            * Se o path for reservado, ignora.
            */
            try
            {
                var extension = VirtualPathUtility.GetExtension(virtualDir);
                if (!string.IsNullOrEmpty(extension))
                    return false;

                var path = VirtualPathUtility.RemoveTrailingSlash(VirtualPathUtility.ToAbsolute(virtualDir).ToLowerInvariant());
                if (!string.IsNullOrEmpty(path))
                {
                    if (_ignoredDirectories.Any(path.Contains))
                        return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("IsVirtualDir", ex);
            }

            return false;
        }

        public bool IsVirtualFile(string virtualPath)
        {
            /*
             * Se o path n�o possir extension, ignora.
             * Se a extens�o n�o for permitida, ignora.
             * Se o path for reservado, ignora.
             * Se o diretório pai possuir extension, ignora
             */
            try
            {
                var extension = VirtualPathUtility.GetExtension(virtualPath);
                if (string.IsNullOrEmpty(extension))
                    return false;

                var directory = VirtualPathUtility.ToAbsolute(VirtualPathUtility.GetDirectory(virtualPath));

                var dirHasExtension = !string.IsNullOrEmpty(VirtualPathUtility.GetExtension(directory));
                if (dirHasExtension)
                    return false;

                var fileName = VirtualPathUtility.GetFileName(virtualPath);

                if (string.IsNullOrEmpty(fileName))
                    return false;

                if (_ignoredFiles.Any(x => x.Equals(fileName, StringComparison.InvariantCultureIgnoreCase)))
                    return false;

                if (!_allowedExtensions.Any(x => x.Equals(extension, StringComparison.InvariantCultureIgnoreCase)))
                    return false;

                if (_ignoredDirectories.Any(directory.Contains))
                    return false;

                return true;

            }
            catch (Exception ex)
            {
                Trace.TraceError("IsVirtualFile", ex);
            }

            return false;
            //return virtualPath.StartsWith(_virtualRootPath);
        }

        public bool FileExists(string virtualPath)
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

        public bool DirectoryExists(string virtualDir)
        {
            var path = NormalizeFilePath(virtualDir);

            var cacheKey = GetCacheKeyForDir(path);
            var item = CacheWrapper.Get(cacheKey);
            if (item != null)
            {
                return item is CustomVirtualDir;
            }

            var result = _service.DirectoryExistsImpl(path);
            if (result)
            {
                CacheWrapper.Set(cacheKey, GetDirectoryInternal(virtualDir, true));
            }
            else
            {
                CacheWrapper.Set(cacheKey, new DummyVirtualFile(true), 20, false);
            }

            return result;
        }

        public CustomVirtualFile GetFile(string virtualPath)
        {
            return GetFileInternal(virtualPath, false);
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

            item = new CustomVirtualFile(virtualPath, bytes);
            if (!isSettingCache)
                CacheWrapper.Set(cacheKey, item);

            return item;
        }

        public CustomVirtualDir GetDirectory(string virtualDir)
        {
            return GetDirectoryInternal(virtualDir, false);
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

        public string GetFileHash(string virtualPath)
        {
            var path = NormalizeFilePath(virtualPath);
            var cacheKey = GetCacheKeyForHash(path);

            string item = CacheWrapper.Get(cacheKey);
            if (item != null)
            {
                return item;
            }

            item = _service.GetFileHash(path);

            CacheWrapper.Set(cacheKey, item);

            return item;
        }

        public IEnumerable<VirtualFileBase> LazyGetChildren(int key)
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

        public static string GetCacheKeyForFile(string path)
        {
            //f = file
            return string.Format("F{0}:{1}", CacheKeySalt, path);
        }

        public static string GetCacheKeyForDir(string path)
        {
            //d = dir
            return string.Format("D{0}:{1}", CacheKeySalt, path);
        }

        public static string GetCacheKeyForHash(string path)
        {
            //h = hash
            return string.Format("H{0}:{1}", CacheKeySalt, path);
        }

        public static void RemoveFileFromCache(string path, bool isDir)
        {
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

        public static string NormalizeFilePath(string virtualPath)
        {
            var absolute = VirtualPathUtility.ToAbsolute(virtualPath);

            var result = VirtualPathUtility.RemoveTrailingSlash(absolute);

            return result.ToLowerInvariant();
        }

    }
}