using System;
using System.Diagnostics;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Roslyn.Compilers;

namespace MvcFromDbMinimal
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //Kompiler.DefaultReferences.AddRange(
            //   new[]
            //    {
            //        new MetadataFileReference(typeof (ApplicationUser).Assembly.Location),
            //        new MetadataFileReference(typeof (ApplicationUserManager).Assembly.Location),
            //    });
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}