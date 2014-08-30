using System;

namespace MvcLib.Common
{
    public class LogEvent : System.Web.Management.WebRequestErrorEvent
    {
        public static void Raise(string message, Exception ex = null)
        {
            new LogEvent(message, ex).Raise();
        }

        private LogEvent(string message, Exception exception)
            : base(message, null, 100001, exception)
        {
        }
    }
}