using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Hosting;

namespace MvcLib.CustomVPP
{
    public class LazyDbFileSystemProviderImpl : AbstractFileSystemProvider
    {
        private readonly IDbService _service;
        readonly Dictionary<string, bool> _cache = new Dictionary<string, bool>();

        private readonly string _path;

        public LazyDbFileSystemProviderImpl(IDbService service)
        {
            _service = service;
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

            if (!_cache.ContainsKey(virtualPath))
            {
                var r = _service.FileExistsImpl(cacheKey);
                _cache[cacheKey] = r;
                return r;
            }

            return _cache[cacheKey];
        }

        public override string GetFileHash(string virtualPath)
        {
            var cacheKey = NormalizeFilePath(virtualPath);
            return _service.GetFileHash(cacheKey);
        }

        public override CustomVirtualFile GetFile(string virtualPath)
        {
            var cacheKey = NormalizeFilePath(virtualPath);
            var path = KeyToCache(cacheKey);
            byte[] bytes;

            if (!File.Exists(path))
            {
                bytes = _service.GetFileBytes(cacheKey);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllBytes(path, bytes);
            }
            else
            {
                bytes = File.ReadAllBytes(path);
            }
            return new CustomVirtualFile(virtualPath, bytes, DateTime.Now.ToString());
        }

        public override bool DirectoryExists(string virtualDir)
        {
            return false;
        }

        public override CustomVirtualDir GetDirectory(string virtualDir)
        {
            return null;
        }

        public override IEnumerable<VirtualFileBase> LazyGetChildren(int key)
        {
            return null;
        }
    }
}