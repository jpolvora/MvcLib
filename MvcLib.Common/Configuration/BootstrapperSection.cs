using System;
using System.Configuration;
using System.Diagnostics;

namespace MvcLib.Common.Configuration
{
    //singleton static
    public class BootstrapperSection : ConfigurationSection
    {
        private static BootstrapperSection _instance;

        public static BootstrapperSection Initialize()
        {
            try
            {
                Instance = (BootstrapperSection)ConfigurationManager.GetSection("MvcLib");
            }
            catch (Exception ex)
            {
                Trace.TraceInformation("Exception Reading MvcLib section configuration: {0}", ex.Message);
            }
            finally
            {
                Trace.TraceInformation("Reading MvcLib section configuration: {0}", Instance != null);
            }
            return Instance;
        }

        public static BootstrapperSection Instance
        {
            get
            {
                {
                    return _instance ?? (_instance = Initialize());
                }
            }
            private set { _instance = value; }
        }

        [ConfigurationProperty("stopMonitoring", DefaultValue = false)]
        public bool StopMonitoring
        {
            get { return (Boolean)this["stopMonitoring"]; }
            set { this["stopMonitoring"] = value; }
        }

        [ConfigurationProperty("insertroutes", DefaultValue = false)]
        public bool InsertRoutes
        {
            get { return (Boolean)this["insertroutes"]; }
            set { this["insertroutes"] = value; }
        }

        [ConfigurationProperty("verbose", DefaultValue = false)]
        public bool Verbose
        {
            get { return (Boolean)this["verbose"]; }
            set { this["verbose"] = value; }
        }

        [ConfigurationProperty("traceoutput", DefaultValue = "~/traceoutput.log")]
        public string TraceOutput
        {
            get { return (string)this["traceoutput"]; }
            set { this["traceoutput"] = value; }
        }

        [ConfigurationProperty("httpmodules")]
        public HttpModulesElement HttpModules
        {
            get { return (HttpModulesElement)this["httpmodules"]; }
            set { this["httpmodules"] = value; }
        }

        [ConfigurationProperty("mvctrace")]
        public MvcTraceElement MvcTrace
        {
            get { return (MvcTraceElement)this["mvctrace"]; }
            set { this["mvctrace"] = value; }
        }

        [ConfigurationProperty("dbfilecontext")]
        public DbFileContextElement DbFileContext
        {
            get { return (DbFileContextElement)this["dbfilecontext"]; }
            set { this["dbfilecontext"] = value; }
        }

        [ConfigurationProperty("dumptolocal")]
        public DumpToLocalElement DumpToLocal
        {
            get { return (DumpToLocalElement)this["dumptolocal"]; }
            set { this["dumptolocal"] = value; }
        }

        [ConfigurationProperty("kompiler")]
        public KompilerElement Kompiler
        {
            get { return (KompilerElement)this["kompiler"]; }
            set { this["kompiler"] = value; }
        }

        [ConfigurationProperty("pluginloader")]
        public PluginLoaderElementBase PluginLoader
        {
            get { return (PluginLoaderElementBase)this["pluginloader"]; }
            set { this["pluginloader"] = value; }
        }

        [ConfigurationProperty("virtualpathproviders")]
        public VirtualPathProviderElement VirtualPathProviders
        {
            get { return (VirtualPathProviderElement)this["virtualpathproviders"]; }
            set { this["virtualpathproviders"] = value; }
        }

        [ConfigurationProperty("mail")]
        public MailConfig Mail
        {
            get { return (MailConfig)this["mail"]; }
            set { this["mail"] = value; }
        }
    }
}