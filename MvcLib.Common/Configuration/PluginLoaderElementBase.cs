using System.Configuration;

namespace MvcLib.Common.Configuration
{
    public class PluginLoaderElementBase : BooleanElementBase
    {
        [ConfigurationProperty("loadfromdb", DefaultValue = false)]
        public bool LoadFromDb
        {
            get { return (bool)this["loadfromdb"]; }
            set { this["loadfromdb"] = value; }
        }

        [ConfigurationProperty("deletefiles", DefaultValue = false)]
        public bool DeleteFiles
        {
            get { return (bool)this["deletefiles"]; }
            set { this["deletefiles"] = value; }
        }
    }
}