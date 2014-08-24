using System.Configuration;

namespace MvcFromDb.Infra.Misc
{
    public static class Config
    {
        public static T ValueOrDefault<T>(string key, T defaultValue)
        {
            key = "custom:" + key;
            var cfgValue = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrEmpty(cfgValue))
            {
                return defaultValue;
            }

            return cfgValue.As<T>();
        }
    }
}