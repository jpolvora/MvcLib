using System.Web;
using System.Web.Routing;

namespace MvcLib.Common.Mvc
{
    public static class HttpHandlerExtensions
    {
        public static void MapHttpHandler<THandler>(this RouteCollection routes, string url)
            where THandler : IHttpHandler, new()
        {
            routes.MapHttpHandler<THandler>(null, url, null, null);
        }

        public static void MapHttpHandler<THandler>(this RouteCollection routes, string name, string url, object defaults, object constraints)
            where THandler : IHttpHandler, new()
        {
            var route = new Route(url, new HttpHandlerRouteHandler<THandler>())
            {
                Defaults = new RouteValueDictionary(defaults),
                Constraints = new RouteValueDictionary(constraints)
            };
            routes.Add(name, route);
        }
    }
}