using System.Web.Mvc;
using MvcLib.Common.Mvc;

namespace MvcFromDb
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new MvcTracerFilter());
        }
    }
}
