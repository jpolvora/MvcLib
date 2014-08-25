using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using MvcFromDb.Infra;
using MvcFromDb.Infra.Entities;
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

            //var factory = DependencyResolver.Current.GetService<IControllerFactory>();
            //var f = ControllerBuilder.Current.GetControllerFactory();
            //Debug.Write(object.Equals(factory, f));
            //ControllerBuilder.Current.SetControllerFactory(factory);
        }
    }
}
