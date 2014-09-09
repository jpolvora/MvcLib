using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.WebPages;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using MvcLib.Bootstrapper;
using MvcLib.Common;
using MvcLib.Common.Cache;
using MvcLib.Common.Configuration;
using MvcLib.Common.Mvc;
using MvcLib.CustomVPP;
using MvcLib.CustomVPP.Impl;
using MvcLib.CustomVPP.RemapperVpp;
using MvcLib.DbFileSystem;
using MvcLib.FsDump;
using MvcLib.HttpModules;
using MvcLib.Kompiler;
using MvcLib.PluginLoader;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(Init), "PreStart")]
[assembly: WebActivatorEx.PostApplicationStartMethod(typeof(Init), "PostStart")]

namespace MvcLib.Bootstrapper
{
    public class Init
    {
        private static string _traceFileName;
        private static bool _initialized;

        public static void PreStart()
        {
            using (DisposableTimer.StartNew("PRE_START"))
            {
                var cfg = BootstrapperSection.Initialize();

                //cria um text logger somente para o startup
                //remove no post start
                try
                {
                    _traceFileName = HostingEnvironment.MapPath(cfg.TraceOutput);
                    if (File.Exists(_traceFileName))
                        File.Delete(_traceFileName);

                    var listener = new TextWriterTraceListener(_traceFileName, "StartupListener");

                    Trace.Listeners.Add(listener);
                }
                catch { }

                var executingAssembly = Assembly.GetExecutingAssembly();
                Trace.TraceInformation("Entry Assembly: {0}", executingAssembly.GetName().Name);

                if (cfg.HttpModules.Trace.Enabled)
                {
                    DynamicModuleUtility.RegisterModule(typeof(TracerHttpModule));
                }

                if (cfg.StopMonitoring)
                {
                    HttpInternals.StopFileMonitoring();
                }

                if (cfg.HttpModules.CustomError.Enabled)
                {
                    DynamicModuleUtility.RegisterModule(typeof(CustomErrorHttpModule));
                }

                if (cfg.HttpModules.WhiteSpace.Enabled)
                {
                    DynamicModuleUtility.RegisterModule(typeof(WhitespaceModule));
                }

                using (DisposableTimer.StartNew("DbFileContext"))
                {
                    DbFileContext.Initialize();
                }

                if (cfg.PluginLoader.Enabled)
                {
                    using (DisposableTimer.StartNew("PluginLoader"))
                    {
                        PluginLoaderEntryPoint.Initialize();
                    }
                }

                if (cfg.VirtualPathProviders.SubFolderVpp.Enabled)
                {
                    SubfolderVpp.SelfRegister();
                }

                if (cfg.DumpToLocal.Enabled)
                {
                    using (DisposableTimer.StartNew("DumpToLocal"))
                    {
                        DbToLocal.Execute();
                    }
                }

                //todo: Dependency Injection
                if (cfg.VirtualPathProviders.DbFileSystemVpp.Enabled)
                {
                    var customvpp = new CustomVirtualPathProvider()
                        .AddImpl(new CachedDbServiceFileSystemProvider(new DefaultDbService(), new WebCacheWrapper()));
                    HostingEnvironment.RegisterVirtualPathProvider(customvpp);
                }

                KompilerEntryPoint.AddReferences(
                    typeof(Controller),
                    typeof(WebPageRenderingBase),
                    typeof(WebCacheWrapper),
                    typeof(ViewRenderer),
                    typeof(DbToLocal),
                    typeof(ErrorModel));

                if (cfg.Kompiler.Enabled)
                {
                    using (DisposableTimer.StartNew("Kompiler"))
                    {
                        KompilerEntryPoint.Execute();
                    }
                }

                if (cfg.InsertRoutes)
                {
                    var routes = RouteTable.Routes;

                    routes.RouteExistingFiles = false;
                    routes.LowercaseUrls = true;
                    routes.AppendTrailingSlash = true;

                    routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
                    routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.ico(/.*)?" });
                    routes.IgnoreRoute("{*staticfile}", new { staticfile = @".*\.(css|js|txt|png|gif|jpg|jpeg|bmp)(/.*)?" });

                    routes.IgnoreRoute("Content/{*pathInfo}");
                    routes.IgnoreRoute("Scripts/{*pathInfo}");
                    routes.IgnoreRoute("Bundles/{*pathInfo}");

                    //routes.MapRoute("MvcLib", "{controller}/{action}", new string[] { "" });


                    if (cfg.TraceOutput.IsNotNullOrWhiteSpace())
                    {
                        //routes.MapHttpHandler<WebPagesRouteHandler>("~/dump.cshtml");
                    }
                }
            }
        }

        public static void PostStart()
        {
            if (_initialized)
                return;

            _initialized = true;

            using (DisposableTimer.StartNew("POST_START ..."))
            {
                Trace.TraceInformation("Debugging Enabled: {0}", HttpContext.Current.IsDebuggingEnabled);
                Trace.TraceInformation("CustomErrors Enabled: {0}", HttpContext.Current.IsCustomErrorEnabled);
                var commitId = Config.ValueOrDefault("appharbor.commit_id", "");
                Trace.TraceInformation("Commit Id: {0}", commitId);

                var cfg = BootstrapperSection.Instance;

                if (cfg.MvcTrace.Enabled)
                {
                    GlobalFilters.Filters.Add(new MvcTracerFilter());
                }

                if (cfg.Verbose)
                {
                    var application = HttpContext.Current.ApplicationInstance;

                    var modules = application.Modules;
                    foreach (var module in modules)
                    {
                        Trace.TraceInformation("Module Loaded: {0}", module);
                    }
                }

                //dump routes
                var routes = RouteTable.Routes;

                if (cfg.Verbose)
                {
                    var i = routes.Count;
                    Trace.TraceInformation("Found {0} routes in RouteTable", i);

                    foreach (var routeBase in routes)
                    {
                        var route = (Route)routeBase;
                        Trace.TraceInformation("Handler: {0} at URL: {1}", route.RouteHandler, route.Url);
                    }
                }

                //viewengine locations
                var mvcroot = cfg.DumpToLocal.Folder;

                var razorViewEngine = ViewEngines.Engines.OfType<RazorViewEngine>().FirstOrDefault();
                if (razorViewEngine != null)
                {
                    Trace.TraceInformation("Configuring RazorViewEngine Location Formats");
                    var vlf = new string[]
                    {
                        mvcroot + "/Views/{1}/{0}.cshtml",
                        mvcroot + "/Views/Shared/{0}.cshtml",
                    };
                    razorViewEngine.ViewLocationFormats = razorViewEngine.ViewLocationFormats.Extend(false, vlf);

                    var mlf = new string[]
                    {
                        mvcroot + "/Views/{1}/{0}.cshtml",
                        mvcroot + "/Views/Shared/{0}.cshtml",
                    };
                    razorViewEngine.MasterLocationFormats = razorViewEngine.MasterLocationFormats.Extend(false, mlf);

                    var plf = new string[]
                    {
                        mvcroot + "/Views/{1}/{0}.cshtml",
                        mvcroot + "/Views/Shared/{0}.cshtml",
                    };
                    razorViewEngine.PartialViewLocationFormats = razorViewEngine.PartialViewLocationFormats.Extend(false, plf);

                    var avlf = new string[]
                    {
                        mvcroot + "/Areas/{2}/Views/{1}/{0}.cshtml",
                        mvcroot + "/Areas/{2}/Views/Shared/{0}.cshtml",
                    };
                    razorViewEngine.AreaViewLocationFormats = razorViewEngine.AreaViewLocationFormats.Extend(false, avlf);

                    var amlf = new string[]
                    {
                        mvcroot + "/Areas/{2}/Views/{1}/{0}.cshtml",
                        mvcroot + "/Areas/{2}/Views/Shared/{0}.cshtml",
                    };
                    razorViewEngine.AreaMasterLocationFormats = razorViewEngine.AreaMasterLocationFormats.Extend(false, amlf);

                    var apvlf = new string[]
                    {
                        mvcroot + "/Areas/{2}/Views/{1}/{0}.cshtml",
                        mvcroot + "/Areas/{2}/Views/Shared/{0}.cshtml",
                    };
                    razorViewEngine.AreaPartialViewLocationFormats = razorViewEngine.AreaPartialViewLocationFormats.Extend(false, apvlf);

                    if (cfg.Verbose)
                    {
                        foreach (var locationFormat in razorViewEngine.ViewLocationFormats)
                        {
                            Trace.WriteLine(locationFormat);
                        }
                    }

                    ViewEngines.Engines.Clear();
                    ViewEngines.Engines.Add(razorViewEngine);
                }
                else
                {
                    Trace.TraceInformation("Cannot Configure RazorViewEngine: View Engine not found");
                }


                Trace.Flush();
                var listener = Trace.Listeners["StartupListener"] as TextWriterTraceListener;
                if (listener != null)
                {
                    listener.Flush();
                    listener.Close();
                    Trace.Listeners.Remove(listener);
                }

                //envia log de startup por email
                if (cfg.Mail.SendStartupLog && !Config.IsInDebugMode)
                {
                    try
                    {
                        if (!File.Exists(_traceFileName)) return;
                        var txt = File.ReadAllText(_traceFileName);
                        var body = txt + "\r\n";

                        using (var client = new SmtpClient())
                        {
                            var msg = new MailMessage(
                                new MailAddress(cfg.Mail.MailAdmin, "Admin"),
                                new MailAddress(cfg.Mail.MailDeveloper))
                            {
                                Subject = "App Startup Log",
                                IsBodyHtml = false,
                                BodyEncoding = Encoding.UTF8,
                                Body =  ""
                            };
                            
                            var alternate = AlternateView.CreateAlternateViewFromString(body,
                                new ContentType("text/plain"));
                            msg.AlternateViews.Add(alternate);

                            //msg.Attachments.Add(new Attachment(_traceFileName));
                            client.Send(msg);
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError(ex.Message);
                    }
                }
            }
        }
    }
}