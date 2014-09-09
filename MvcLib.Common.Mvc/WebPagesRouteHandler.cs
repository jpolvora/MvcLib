using System.Diagnostics;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.WebPages;

namespace MvcLib.Common.Mvc
{
    public class WebPagesRouteHandler : IRouteHandler
    {
        private readonly string _virtualPath;
        private Route _routeVirtualPath;

        public WebPagesRouteHandler(string virtualPath)
        {
            if (Path.HasExtension(virtualPath))
                _virtualPath = virtualPath;
            else _virtualPath = Path.ChangeExtension(virtualPath, "cshtml");

            //WebRazorHostFactory.CreateDefaultHost()

            Trace.TraceInformation("Route added: {0}", virtualPath);
        }

        private Route RouteVirtualPath
        {
            get
            {
                if (_routeVirtualPath == null)
                {
                    _routeVirtualPath = new Route(_virtualPath.Substring(2), this);
                }
                return this._routeVirtualPath;
            }
        }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            var substitutedVirtualPath = GetSubstitutedVirtualPath(requestContext);
            int index = substitutedVirtualPath.IndexOf('?');
            if (index != -1)
            {
                substitutedVirtualPath = substitutedVirtualPath.Substring(0, index);
            }

            var handler = WebPageHttpHandler.CreateFromVirtualPath(substitutedVirtualPath);

            Trace.TraceInformation("Routing {0} = {1}", this._virtualPath, handler);

            if (handler != null)
                return handler;

            return new MvcHandler(requestContext);

            //return WebPageHttpHandler.CreateFromVirtualPath("~/db/Error.cshtml");
        }

        public string GetSubstitutedVirtualPath(RequestContext requestContext)
        {
            var virtualPath = RouteVirtualPath.GetVirtualPath(requestContext, requestContext.RouteData.Values);
            if (virtualPath == null)
            {
                return _virtualPath;
            }
            return ("~/" + virtualPath.VirtualPath);
        }
    }
}