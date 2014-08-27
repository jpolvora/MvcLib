using System.Diagnostics;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using MvcFromDb.Models;
using Roslyn.Compilers;

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
