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
            var listener = new TextWriterTraceListener(traceOutput, "StartupListener")
            {
                TraceOutputOptions = TraceOptions.DateTime
            };

            Trace.Listeners.Add(listener);
            Trace.AutoFlush = true;

            Assembly web = Assembly.GetExecutingAssembly();
            AssemblyName webName = web.GetName();

            Trace.TraceInformation("RUNNING PRE_START ... Assembly: {0}", webName);

            DynamicModuleUtility.RegisterModule(typeof(TracerHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(CustomErrorHttpModule));

            DbFileContext.Initialize();

            var customvpp = new CustomVirtualPathProvider()
                .AddImpl(new CachedDbServiceFileSystemProvider(new DefaultDbService(), new FileSystemCache()));
            HostingEnvironment.RegisterVirtualPathProvider(customvpp);


            //EntropiaSection.Initialize();

            //todo: CHECK if is Full Trust

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

            GlobalFilters.Filters.Add(new MvcTracerFilter());

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

            Trace.Listeners.Remove("StartupListener");
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
            Trace.Indent();
            foreach (var type in types)
            {
                Trace.WriteLine(type.Name);
            }
            Trace.Unindent();

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