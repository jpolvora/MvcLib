using System;
using System.Configuration;

namespace MvcLib.Common.Configuration
{
    public class MailConfig : ConfigurationElement
    {
        [ConfigurationProperty("admin")]
        public string MailAdmin
        {
            get { return (string)this["admin"]; }
            set { this["admin"] = value; }
        }

        [ConfigurationProperty("developer")]
        public string MailDeveloper
        {
            get { return (string)this["developer"]; }
            set { this["developer"] = value; }
        }

        [ConfigurationProperty("sendstartuplog", DefaultValue = false)]
        public bool SendStartupLog
        {
            get { return (Boolean)this["sendstartuplog"]; }
            set { this["sendstartuplog"] = value; }
        }

        [ConfigurationProperty("sendexceptiontodeveloper", DefaultValue = false)]
        public bool SendExceptionToDeveloper
        {
            get { return (Boolean)this["sendexceptiontodeveloper"]; }
            set { this["sendexceptiontodeveloper"] = value; }
        }
    }
}