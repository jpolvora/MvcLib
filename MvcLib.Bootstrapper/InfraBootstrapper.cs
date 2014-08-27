using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
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
using MvcLib.PluginCompiler;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(InfraBootstrapper), "PreStart")]
[assembly: WebActivatorEx.PostApplicationStartMethod(typeof(InfraBootstrapper), "PostStart")]

namespace MvcLib.Bootstrapper
{
    public class InfraBootstrapper
    {
        private static bool _initialized;

        public static void PreStart()
        {
            var traceOutput = HostingEnvironment.MapPath("~/traceOutput.log");
            var listener = new TextWriterTraceListener(traceOutput, "StartupListener");

            Trace.Listeners.Add(listener);
            Trace.AutoFlush = true;

            Assembly web = Assembly.GetExecutingAssembly();

            Trace.TraceInformation("Custom Framework RUNNING PRE_START ... Entry: {0}", web.GetName());

            if (Config.ValueOrDefault("TracerHttpModule", true))
            {
                DynamicModuleUtility.RegisterModule(typeof(TracerHttpModule));
            }
            if (Config.ValueOrDefault("CustomErrorHttpModule", true))
            {
                DynamicModuleUtility.RegisterModule(typeof(CustomErrorHttpModule));
            }
            DbFileContext.Initialize();

            if (Config.ValueOrDefault("CustomVPP", false))
            {
                //var customvpp = new RemapVpp();
                //.AddImpl(new LazyDbFileSystemProviderImpl());
                //.AddImpl(new CachedDbServiceFileSystemProvider(new DefaultDbService(), new WebCacheWrapper()));
                //HostingEnvironment.RegisterVirtualPathProvider(customvpp);
            }

            if (Config.ValueOrDefault("DumpToLocal", false))
            {
                DbToLocal.Execute();
            }

            if (AppDomain.CurrentDomain.IsFullyTrusted)
            {
                AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
            }
            else
            {
                Trace.TraceWarning("We are not in FULL TRUST! We must use private probing path in Web.Config");
            }

            AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;

            var forceRecompilation = Config.ValueOrDefault("CustomForceRecompilation", true);
            PluginLoader.Initialize(forceRecompilation);

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

        public static void PostStart()
        {
            if (_initialized)
                return;

            _initialized = true;

            Trace.TraceInformation("RUNNING POST_START ...");

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

            if (!Config.ValueOrDefault("Environment", "Debug").Equals("Debug", StringComparison.OrdinalIgnoreCase))
            {
                Trace.Listeners.Remove("StartupListener");
            }
        }

        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.RequestingAssembly != null)
                return args.RequestingAssembly;

            var ass = PluginStorage.FindAssembly(args.Name);
            if (ass != null)
                Trace.TraceInformation("Assembly found and resolved: {0} = {1}", ass.FullName, ass.Location);
            return ass;
        }

        private static void OnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            if (args.LoadedAssembly.GlobalAssemblyCache)
                return;

            if (args.LoadedAssembly.IsDynamic)
            {
                Trace.TraceInformation("DYNAMIC Assembly Loaded... {0}", args.LoadedAssembly.Location);
                return;
            }

            Trace.TraceInformation("Assembly Loaded... {0}", args.LoadedAssembly.Location);

            var types = args.LoadedAssembly.GetExportedTypes();

            foreach (var type in types)
            {
                Trace.TraceInformation("Type exported: {0}", type.FullName);
            }

            var path = Path.GetDirectoryName(args.LoadedAssembly.Location);
            if (path.IndexOf(PluginLoader.PluginFolder.FullName, 0, StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                try
                {
                    PluginStorage.Register(args.LoadedAssembly);
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.Message);
                }
            }
        }
    }
}