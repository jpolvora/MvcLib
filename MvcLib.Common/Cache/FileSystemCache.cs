using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Web.Hosting;

namespace MvcLib.Common.Cache
{
    public class FileSystemCache : ICacheProvider
    {
        private readonly ConcurrentDictionary<string, bool> _cacheKeys = new ConcurrentDictionary<string, bool>();

        private readonly string _path;
        public FileSystemCache()
        {
            _path = HostingEnvironment.MapPath("~/_files");
            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);
        }

        private string KeyToCache(string cacheKey)
        {
            if (Path.IsPathRooted(cacheKey))
                return cacheKey;

            return _path + cacheKey.Substring(1).Replace("/", "\\");
        }

        public bool HasValue(string key)
        {
            key = KeyToCache(key);

            bool hasValue;
            var r = _cacheKeys.TryGetValue(key, out hasValue);
            return r && hasValue;
        }

        public object Get(string key)
        {
            key = KeyToCache(key);

            if (HasValue(key))
                return File.ReadAllBytes(key);
            return null;
        }

        public T Set<T>(string key, T value, int duration = 20, bool sliding = true) where T : class
        {
            key = KeyToCache(key);

            var file = value as ICacheableBytes;
            if (file != null)
            {
                _cacheKeys.AddOrUpdate(key, s => value != null, (s, b) => true);
                File.WriteAllBytes(key, file.Bytes);
            }
            else
            {
                _cacheKeys.AddOrUpdate(key, s => value != null, (s, b) => false);
            }

            return value;
        }

        public object Remove(string key)
        {
            key = KeyToCache(key);

            bool r;
            _cacheKeys.TryRemove(key, out r);
            if (r)
            {
                var file = File.ReadAllBytes(key);
                try
                {
                    File.Delete(key);
                }
                catch (Exception) { }
                return file;
            }
            return null;
        }

        public void Clear()
        {
            var keys = _cacheKeys.Keys.ToList();
            foreach (var key in keys)
            {
                var skey = KeyToCache(key);
                bool r;
                _cacheKeys.TryRemove(skey, out r);
                try
                {
                    File.Delete(key);
                }
                catch (Exception) { }
            }
        }
    }
}