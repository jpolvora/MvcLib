using System;
using System.Configuration;
using System.Diagnostics;

namespace MvcLib.Common
{
    public static class Config
    {
        public static T ValueOrDefault<T>(string key, T defaultValue)
        {
            var cfgValue = ConfigurationManager.AppSettings[key];

            Trace.TraceInformation("[Config]: Value for '{0}' is '{1}' (default is {2})", key, cfgValue, defaultValue);
            if (string.IsNullOrEmpty(cfgValue))
            {
                return defaultValue;
            }

            return cfgValue.As<T>();
        }

        public static bool IsInDebugMode { get; private set; }

        static Config()
        {
            IsInDebugMode = ValueOrDefault("Environment", "Debug").Equals("Debug", StringComparison.OrdinalIgnoreCase);
        }
    }
}