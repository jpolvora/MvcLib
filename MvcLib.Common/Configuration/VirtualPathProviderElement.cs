using System.Configuration;

namespace MvcLib.Common.Configuration
{
    public class VirtualPathProviderElement : ConfigurationElement
    {
        [ConfigurationProperty("subfolder")]
        public SubFolderVppElement SubFolderVpp
        {
            get { return (SubFolderVppElement)this["subfolder"]; }
            set { this["subfolder"] = value; }
        }

        [ConfigurationProperty("dbfsvpp")]
        public DbFsVppElement DbFileSystemVpp
        {
            get { return (DbFsVppElement)this["dbfsvpp"]; }
            set { this["dbfsvpp"] = value; }
        }
    }
}