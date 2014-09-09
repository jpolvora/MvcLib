using System.Configuration;

namespace MvcLib.Common.Configuration
{
    public class TraceElement : BooleanElementBase
    {
        [ConfigurationProperty("events", DefaultValue = "")]
        public string Events
        {
            get { return (string)this["events"]; }
            set { this["events"] = value; }
        }
    }
}