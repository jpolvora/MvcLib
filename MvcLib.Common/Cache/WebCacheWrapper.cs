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
            Enabled = Config.ValueOrDefault("WebCacheWrapper", true);

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
        public bool HasValue(string key)
        {
            bool hasValue;
            var r = _cacheKeys.TryGetValue(key, out hasValue);
            return r && hasValue;
        }

        public object Get(string key)
        {
            return Enabled ? WebCache.Get(key) : null;
        }
        
        public T Set<T>(string key, T value, int duration = 20, bool sliding = true)
            where T: class
        {
            if (string.IsNullOrEmpty(key))
                return default(T);

            if (value == null)  
                return default(T);

            _cacheKeys.AddOrUpdate(key, s => value != null, (s, b) => value != null);
            if (Enabled)
            {
                WebCache.Set(key, value, duration, sliding);
            }

            return value;
        }

        public object Remove(string key)
        {
            bool r;
            _cacheKeys.TryRemove(key, out r);

            return Enabled ? WebCache.Remove(key) : null;
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