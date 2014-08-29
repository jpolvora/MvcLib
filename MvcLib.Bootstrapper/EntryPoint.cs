using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.WebPages;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using MvcLib.Common;
using MvcLib.Common.Cache;
using MvcLib.Common.Mvc;
using MvcLib.CustomVPP;
using MvcLib.CustomVPP.Impl;
using MvcLib.CustomVPP.RemapperVpp;
using MvcLib.DbFileSystem;
using MvcLib.FsDump;
using MvcLib.HttpModules;
using MvcLib.PluginLoader;
using EntryPoint = MvcLib.Bootstrapper.EntryPoint;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(EntryPoint), "PreStart")]
[assembly: WebActivatorEx.PostApplicationStartMethod(typeof(EntryPoint), "PostStart")]

namespace MvcLib.Bootstrapper
{
    public class EntryPoint
    {
        private static bool _initialized;

        public static void PreStart()
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            Trace.TraceInformation("Entry: {0}", executingAssembly.GetName().Name);

            if (Config.IsInDebugMode)
            {
                var traceOutput = HostingEnvironment.MapPath("~/traceOutput.log");
                if (File.Exists(traceOutput))
                    File.Delete(traceOutput);

                var listener = new TextWriterTraceListener(traceOutput, "StartupListener");

                Trace.Listeners.Add(listener);
                Trace.AutoFlush = true;
            }


            using (DisposableTimer.StartNew("PRE_START"))
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

                //plugin loader deve ser utilizado se dump to local = true ou se utilizar o custom vpp
                if (Config.ValueOrDefault("PluginLoader", false))
                {
                    PluginLoader.EntryPoint.Initialize();
                }

                if (Config.ValueOrDefault("DumpToLocal", false))
                {
                    var customvpp = new SubfolderVpp();
                    HostingEnvironment.RegisterVirtualPathProvider(customvpp);

                    DbToLocal.Execute();
                }
                else if (Config.ValueOrDefault("CustomVirtualPathProvider", false))
                {
                    var customvpp = new CustomVirtualPathProvider()
                        .AddImpl(new CachedDbServiceFileSystemProvider(new DefaultDbService(), new WebCacheWrapper()));
                    HostingEnvironment.RegisterVirtualPathProvider(customvpp);
                }


                if (Config.ValueOrDefault("Kompiler", false))
                {
                    if (Config.ValueOrDefault("KompilerForceRecompilation", false))
                    {
                        //se forçar a recompilação, remove o assembly existente.
                        Kompiler.KompilerDbService.RemoveExistingCompiledAssemblyFromDb();
                    }

                    //se já houver um assembly compilado, não executa a compilação
                    if (!Kompiler.KompilerDbService.ExistsCompiledAssembly())
                    {
                        //EntryPoint depends on PluginLoader, so, initializes it if not previously initialized.
                        PluginLoader.EntryPoint.Initialize();

                        Kompiler.EntryPoint.AddReferences(typeof(Controller), typeof(WebPageRenderingBase), typeof(WebCacheWrapper), typeof(ViewRenderer), typeof(DbToLocal), typeof(CustomErrorHttpModule.ErrorModel));
                        Kompiler.EntryPoint.AddReferences(PluginStorage.GetAssemblies().ToArray());

                        Kompiler.EntryPoint.Execute();
                    }
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

                if (!Config.IsInDebugMode)
                {
                    Trace.Listeners.Remove("StartupListener");
                }
            }
        }
    }
}