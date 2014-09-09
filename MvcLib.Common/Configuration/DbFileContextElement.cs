using System.Configuration;

namespace MvcLib.Common.Configuration
{
    public class DbFileContextElement : ConfigurationElement
    {
        [ConfigurationProperty("connectionstring", DefaultValue = "DbFileContext")]
        public string ConnectionStringKey
        {
            get { return (string)this["connectionstring"]; }
            set { this["connectionstring"] = value; }
        }

        [ConfigurationProperty("key", DefaultValue = "DbFileContextMigrationConfiguration")]
        public string MigrationKey
        {
            get { return (string)this["key"]; }
            set { this["key"] = value; }
        }

        [ConfigurationProperty("verbose", DefaultValue = false)]
        public bool Verbose
        {
            get { return (bool)this["verbose"]; }
            set { this["verbose"] = value; }
        }
    }
}