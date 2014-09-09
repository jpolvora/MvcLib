using System.Configuration;

namespace MvcLib.Common.Configuration
{
    public class KompilerElement : BooleanElementBase
    {
        [ConfigurationProperty("loadfromdb", DefaultValue = false)]
        public bool LoadFromDb
        {
            get { return (bool)this["loadfromdb"]; }
            set { this["loadfromdb"] = value; }
        }

        [ConfigurationProperty("force", DefaultValue = false)]
        public bool ForceRecompilation
        {
            get { return (bool)this["force"]; }
            set { this["force"] = value; }
        }

        [ConfigurationProperty("roslyn", DefaultValue = false)]
        public bool Roslyn
        {
            get { return (bool)this["roslyn"]; }
            set { this["roslyn"] = value; }
        }

        [ConfigurationProperty("assemblyname", DefaultValue = "db-compiled-assembly")]
        public string AssemblyName
        {
            get { return (string)this["assemblyname"]; }
            set { this["assemblyname"] = value; }
        }
    }
}