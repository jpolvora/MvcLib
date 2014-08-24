using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using MvcFromDb.Infra;
using MvcFromDb.Infra.VPP;
using MvcFromDb.Infra.VPP.Impl;

namespace MvcFromDb
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            DbFileContext.Initialize();

            //Trace.AutoFlush = true;

            //Trace.Listeners.Add(new TextWriterTraceListener(Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "tracer.txt")));

            var customvpp = new CustomVirtualPathProvider().AddImpl(new DbVirtualPathProvider(new DefaultDbService()));
            HostingEnvironment.RegisterVirtualPathProvider(customvpp);
        }
    }
}
