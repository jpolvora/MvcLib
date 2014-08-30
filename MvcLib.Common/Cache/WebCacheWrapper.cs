using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Web.Helpers;

namespace MvcLib.Common.Cache
{
    public class WebCacheWrapper : ICacheProvider
    {
        public static bool Enabled { get; private set; }

        private readonly ConcurrentDictionary<string, bool> _cacheKeys = new ConcurrentDictionary<string, bool>();

        public static WebCacheWrapper Instance { get; private set; }
        static WebCacheWrapper()
        {
            Enabled = Config.ValueOrDefault("WebCacheWrapper", false);

            Trace.TraceInformation("Using WebCacheWrapper: {0}", Enabled);
        }

        public WebCacheWrapper()
        {
            Instance = this;
        }

        /// <summary>
        /// Indica se houve uma tentativa de utilizar o cache com a chave, contendo um valor não nulo.
        /// Útil para verificar se uma entrada foi inserida no cache, mesmo que o cache esteja desabilitado.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HasEntry(string key)
        {
            bool hasValue;
            var r = _cacheKeys.TryGetValue(key, out hasValue);
            return r && hasValue;
        }

        public object Get(string key)
        {
            if (!Enabled)
                return null;

            object result = null;

            bool hasValue;
            if (_cacheKeys.TryGetValue(key, out hasValue) && hasValue)
            {
                result = WebCache.Get(key);
                if (result == null)
                {
                    Trace.TraceInformation("[WebCacheWrapper]:Item expired: '{0}'", key);
                    //expirou
                    bool r;
                    _cacheKeys.TryRemove(key, out r);
                }
            }
            return result;
        }

        public T Set<T>(string key, T value, int duration = 20, bool sliding = true)
            where T : class
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key", "key obrigatório");

            _cacheKeys.AddOrUpdate(key, s => value != null, (s, b) => value != null);

            if (!Enabled)
                return value;

            if (value != null)
                WebCache.Set(key, value, duration, sliding);

            return value;
        }

        public void Remove(string key)
        {
            bool r;
            _cacheKeys.TryRemove(key, out r);

            WebCache.Remove(key);
        }

        public void Clear()
        {
            var keys = _cacheKeys.Keys.ToList();
            foreach (var key in keys)
            {
                bool r;
                _cacheKeys.TryRemove(key, out r);
                if (Enabled)
                    WebCache.Remove(key);
            }
        }
    }
}