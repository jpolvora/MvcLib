using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Hosting;

namespace MvcLib.CustomVPP
{
    public class LazyDbFileSystemProviderImpl : AbstractFileSystemProvider
    {
        private readonly string _path;

        public LazyDbFileSystemProviderImpl()
        {
            _path = HostingEnvironment.MapPath("~/_files");
            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);
        }

        private string KeyToCache(string cacheKey)
        {
            if (cacheKey.StartsWith("/"))
                return _path + cacheKey.Replace("/", "\\");
            return cacheKey;
        }

        public override bool FileExists(string virtualPath)
        {
            var cacheKey = NormalizeFilePath(virtualPath);
            var path = KeyToCache(cacheKey);

            return File.Exists(path);
        }

        public override string GetFileHash(string virtualPath)
        {
            return null;
        }

        public override CustomVirtualFile GetFile(string virtualPath)
        {
            var cacheKey = NormalizeFilePath(virtualPath);
            var path = KeyToCache(cacheKey);
            byte[] bytes = File.ReadAllBytes(path);
            
            return new CustomVirtualFile(virtualPath, bytes, DateTime.Now.ToString());
        }

        public override bool DirectoryExists(string virtualDir)
        {
            var cacheKey = NormalizeFilePath(virtualDir);
            var path = KeyToCache(cacheKey);

            return Directory.Exists(path);
        }

        public override CustomVirtualDir GetDirectory(string virtualDir)
        {
            return new CustomVirtualDir(virtualDir, () => LazyGetChildren(0));
        }

        public override IEnumerable<VirtualFileBase> LazyGetChildren(int key)
        {
            return null;
        }
    }
}