using System.Configuration;

namespace MvcLib.Common
{
    public static class Config
    {
        public static T ValueOrDefault<T>(string key, T defaultValue)
        {
            var cfgValue = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrEmpty(cfgValue))
            {
                return defaultValue;
            }

            return cfgValue.As<T>();
        }
    }
}