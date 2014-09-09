using System.Configuration;

namespace MvcLib.Common.Configuration
{
    public class CustomErrorElement : BooleanElementBase
    {
        [ConfigurationProperty("controllername", DefaultValue = "")]
        public string ControllerName
        {
            get { return (string)this["controllername"]; }
            set { this["controllername"] = value; }
        }

        [ConfigurationProperty("errorviewpath", DefaultValue = "~/views/shared/customerror.cshtml")]
        public string ErrorViewPath
        {
            get { return (string)this["errorviewpath"]; }
            set { this["errorviewpath"] = value; }
        }
    }
}