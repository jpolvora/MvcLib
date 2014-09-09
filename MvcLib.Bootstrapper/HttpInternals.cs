using System;
using System.Diagnostics;
using System.DirectoryServices;
using System.Reflection;
using System.Web;

namespace MvcLib.Bootstrapper
{
    internal static class HttpInternals
    {
        /*
          private void StartMonitoringDirectoryRenamesAndBinDirectory() {
            _fcm.StartMonitoringDirectoryRenamesAndBinDirectory(AppDomainAppPathInternal, new FileChangeEventHandler(this.OnCriticalDirectoryChange));
        }
         
         */

        private static readonly FieldInfo s_TheRuntime = typeof(HttpRuntime).GetField("_theRuntime", BindingFlags.NonPublic | BindingFlags.Static);

        private static readonly FieldInfo s_FileChangesMonitor = typeof(HttpRuntime).GetField("_fcm", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo s_FileChangesMonitorStop = s_FileChangesMonitor.FieldType.GetMethod("Stop", BindingFlags.NonPublic | BindingFlags.Instance);

        private static object HttpRuntime
        {
            get
            {
                return s_TheRuntime.GetValue(null);
            }
        }

        private static object FileChangesMonitor
        {
            get
            {
                return s_FileChangesMonitor.GetValue(HttpRuntime);
            }
        }

        public static void StopFileMonitoring()
        {
            Trace.TraceWarning("[HttpInternals]:StopFileMonitoring");
            s_FileChangesMonitorStop.Invoke(FileChangesMonitor, null);
        }

        //public static void StartFileMonitoring()
        //{
        //    //System.Web.HttpRuntime.AppDomainAppPath

        //    var dele = 
        //    s_FileChangesMonitorStart.Invoke(FileChangesMonitor, new object[] { System.Web.HttpRuntime.AppDomainAppPath, OnCriticalDirectoryChange });

        //    //internal void StartMonitoringDirectoryRenamesAndBinDirectory(string dir, FileChangeEventHandler callback)
        //}

        //static void OnCriticalDirectoryChange(Object sender, FileChangeEvent e)
        //{
        //    System.Web.HttpRuntime.UnloadAppDomain();
        //}


        public static string status(string appPoolName)
        {
            string appPoolPath = @"IIS://" + System.Environment.MachineName + "/W3SVC/AppPools/" + appPoolName;
            try
            {
                DirectoryEntry w3svc = new DirectoryEntry(appPoolPath);
                int intStatus = (int)w3svc.InvokeGet("AppPoolState");
                switch (intStatus)
                {
                    case 2:
                        return "Running";
                    case 4:
                        return "Stopped";
                    default:
                        return "Unknown";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static void StopAppPool(string appPoolName)
        {
            string appPoolPath = @"IIS://" + System.Environment.MachineName + "/W3SVC/AppPools/" + appPoolName;
            try
            {
                DirectoryEntry w3svc = new DirectoryEntry(appPoolPath);
                w3svc.Invoke("Stop", null);
                status(appPoolName);
            }
            catch (Exception ex)
            {
            }
        }

        public static void StartAppPool(string appPoolName)
        {
            string appPoolPath = @"IIS://" + System.Environment.MachineName + "/W3SVC/AppPools/" + appPoolName;
            try
            {
                DirectoryEntry w3svc = new DirectoryEntry(appPoolPath);
                w3svc.Invoke("Start", null);
                status(appPoolName);
            }
            catch (Exception ex)
            {
            }
        }
    }
}