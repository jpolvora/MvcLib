using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.WebPages;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using MvcFromDb.Infra;
using MvcLib.Bootstrapper;
using MvcLib.Common;
using MvcLib.Common.Cache;
using MvcLib.Common.Mvc;
using MvcLib.CustomVPP;
using MvcLib.CustomVPP.Impl;
using MvcLib.DbFileSystem;
using MvcLib.FsDump;
using MvcLib.PluginLoader;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(InfraBootstrapper), "PreStart")]
[assembly: WebActivatorEx.PostApplicationStartMethod(typeof(InfraBootstrapper), "PostStart")]

namespace MvcLib.Bootstrapper
{
    public class InfraBootstrapper
    {
        private static bool _initialized;

        public static void PreStart()
        {
            if (Config.IsInDebugMode)
            {
                var traceOutput = HostingEnvironment.MapPath("~/traceOutput.log");
                var listener = new TextWriterTraceListener(traceOutput, "StartupListener");

                Trace.Listeners.Add(listener);
                Trace.AutoFlush = true;
            }

            var executingAssembly = Assembly.GetExecutingAssembly();

            using (DisposableTimer.StartNew(string.Format("Custom Framework RUNNING PRE_START ... Entry: {0}",
                    executingAssembly.GetName().Name)))
            {
                if (Config.ValueOrDefault("TracerHttpModule", true))
                {
                    DynamicModuleUtility.RegisterModule(typeof(TracerHttpModule));
                }
                if (Config.ValueOrDefault("CustomErrorHttpModule", true))
                {
                    DynamicModuleUtility.RegisterModule(typeof(CustomErrorHttpModule));
                }

                DbFileContext.Initialize();

                if (Config.ValueOrDefault("CustomVirtualPathProvider", false))
                {
                    var customvpp = new CustomVirtualPathProvider()
                    .AddImpl(new CachedDbServiceFileSystemProvider(new DefaultDbService(), new WebCacheWrapper()));
                    HostingEnvironment.RegisterVirtualPathProvider(customvpp);
                }

                if (Config.ValueOrDefault("DumpToLocal", false))
                {
                    DbToLocal.Execute();
                }

                if (Config.ValueOrDefault("PluginLoader", false))
                {
                    PluginLoader.PluginLoader.Initialize();
                }

                if (Config.ValueOrDefault("Kompiler", false))
                {
                    //Kompiler depends on PluginLoader, so, initializes it if not previously initialized.
                    PluginLoader.PluginLoader.Initialize();

                    Kompiler.Kompiler.AddReferences(PluginStorage.GetAssemblies().ToArray());
                    Kompiler.Kompiler.AddReferences(typeof(Controller), typeof(WebPageRenderingBase), typeof(WebCacheWrapper), typeof(ViewRenderer), typeof(DbToLocal));

                    Kompiler.Kompiler.Initialize();
                }

                //config routing
                //var routes = RouteTable.Routes;

                //if (EntropiaSection.Instance.InsertRoutesDefaults)
                //{
                //    routes.RouteExistingFiles = false;
                //    routes.LowercaseUrls = true;
                //    routes.AppendTrailingSlash = true;

                //    routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
                //    routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.ico(/.*)?" });
                //    routes.IgnoreRoute("{*staticfile}", new { staticfile = @".*\.(css|js|txt|png|gif|jpg|jpeg|bmp)(/.*)?" });

                //    routes.IgnoreRoute("Content/{*pathInfo}");
                //    routes.IgnoreRoute("Scripts/{*pathInfo}");
                //    routes.IgnoreRoute("Bundles/{*pathInfo}");
                //}

                //if (EntropiaSection.Instance.EnableDumpLog)
                //{
                //    var endpoint = EntropiaSection.Instance.DumpLogEndPoint;
                //    routes.MapHttpHandler<DumpHandler>(endpoint);
                //}
            }
        }

        public static void PostStart()
        {
            if (_initialized)
                return;

            _initialized = true;

            using (DisposableTimer.StartNew("RUNNING POST_START ..."))
            {
                if (Config.ValueOrDefault("MvcTracerFilter", true))
                {
                    GlobalFilters.Filters.Add(new MvcTracerFilter());
                }

                var application = HttpContext.Current.ApplicationInstance;

                var modules = application.Modules;
                foreach (var module in modules)
                {
                    Trace.TraceInformation("Module Loaded: {0}", module);
                }

                //dump routes
                var routes = RouteTable.Routes;

                var i = routes.Count;
                Trace.TraceInformation("Found {0} routes in RouteTable", i);

                foreach (var routeBase in routes)
                {
                    var route = (Route)routeBase;
                    Trace.TraceInformation("Handler: {0} at URL: {1}", route.RouteHandler, route.Url);
                }

                if (Config.IsInDebugMode)
                {
                    Trace.Listeners.Remove("StartupListener");
                }
            }
        }
    }
}