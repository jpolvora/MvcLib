using System.Web;
using System.Web.Routing;

namespace MvcLib.Common.Mvc
{
    public class HttpHandlerRouteHandler<THandler>: IRouteHandler where THandler : IHttpHandler, new()
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new THandler();
        }
    }
}