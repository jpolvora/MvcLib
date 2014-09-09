using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace MvcLib.HttpModules
{
    public class SetCacheHttpModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.PreSendRequestHeaders += ContextOnPreSendRequestHeaders;
        }

        private static void ContextOnPreSendRequestHeaders(object sender, EventArgs eventArgs)
        {
            var application = (HttpApplication)sender;
            var context = application.Context;

            //se não foi especificado um cache.
            //if (context.Response.Headers.AllKeys.Contains("Cache-Control"))
            //    return;

            string file = context.Server.MapPath(context.Request.CurrentExecutionFilePath);

            if (!File.Exists(file))
                return;

            Trace.TraceInformation("[SetCacheHttpModule]: Setting cache/response Header for '{0}'", file);

            var response = context.Response;

            response.Cache.SetLastModified(File.GetLastWriteTimeUtc(file));
            response.Cache.SetValidUntilExpires(true);
            response.Cache.SetExpires(DateTime.Now.AddYears(1));
            response.Cache.SetCacheability(HttpCacheability.ServerAndPrivate);
            response.Cache.SetVaryByCustom("Accept-Encoding");

            response.AddCacheDependency(new CacheDependency(file));
        }

        public void Dispose()
        {

        }
    }
}