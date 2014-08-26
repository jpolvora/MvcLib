﻿using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Web.Helpers;

namespace MvcLib.Common
{
    public static class CacheWrapper
    {
        public static bool Enabled { get; private set; }

        private static readonly ConcurrentDictionary<string, bool> CacheKeys = new ConcurrentDictionary<string, bool>();

        static CacheWrapper()
        {
            Enabled = Config.ValueOrDefault("CustomCacheWrapper", true);

            Trace.TraceInformation("Using CacheWrapper: {0}", Enabled);
        }

        static string GetSaltedKey(string key)
        {
            return key;
        }

        /// <summary>
        /// Indica se houve uma tentativa de utilizar o cache com a chave, contendo um valor não nulo.
        /// Útil para verificar se uma entrada foi inserida no cache, mesmo que o cache esteja desabilitado.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool HasValue(string key)
        {
            var sKey = GetSaltedKey(key);

            bool hasValue;
            var r = CacheKeys.TryGetValue(sKey, out hasValue);
            return r && hasValue;
        }

        public static dynamic Get(string key)
        {
            return Enabled ? WebCache.Get(GetSaltedKey(key)) : null;
        }

        public static T Set<T>(string key, T value, int duration = 20, bool sliding = true)
            where T: class
        {
            if (string.IsNullOrEmpty(key))
                return default(T);

            if (value == null)  
                return default(T);

            var sKey = GetSaltedKey(key);
            CacheKeys.AddOrUpdate(sKey, s => value != null, (s, b) => value != null);
            if (Enabled)
            {
                WebCache.Set(sKey, value, duration, sliding);
            }

            return value;
        }

        public static object Remove(string key)
        {
            var sKey = GetSaltedKey(key);

            bool r;
            CacheKeys.TryRemove(sKey, out r);

            return Enabled ? WebCache.Remove(sKey) : null;
        }

        public static void Clear()
        {
            var keys = CacheKeys.Keys.ToList();
            foreach (var key in keys)
            {
                bool r;
                CacheKeys.TryRemove(key, out r);
                if (Enabled)
                    WebCache.Remove(key);
            }
        }
    }
}