using System.Diagnostics;
using System.Web.Mvc;
using System.Web.Mvc.Filters;

namespace MvcLib.Common.Mvc
{
    public class MvcTracerFilter : ActionFilterAttribute, IAuthorizationFilter, IExceptionFilter, IAuthenticationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            Trace.TraceInformation("[MvcTracerFilter]:[OnAuthorization]: {0}", filterContext.Controller);
        }

        public void OnException(ExceptionContext filterContext)
        {
            Trace.TraceInformation("[MvcTracerFilter]:[OnException]: {0}", filterContext.Exception);
        }

        public void OnAuthentication(AuthenticationContext filterContext)
        {
            Trace.TraceInformation("[MvcTracerFilter]:[OnAuthentication]: {0}", filterContext.Controller);
        }

        public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
        {
            Trace.TraceInformation("[MvcTracerFilter]:[OnAuthenticationChallenge]: {0}", filterContext.Controller);
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Trace.TraceInformation("[MvcTracerFilter]:[OnActionExecuting]: {0}/{1}", filterContext.Controller, filterContext.ActionDescriptor.ActionName);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            Trace.TraceInformation("[MvcTracerFilter]:[OnActionExecuted]: {0}/{1}", filterContext.Controller, filterContext.ActionDescriptor.ActionName);
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            Trace.TraceInformation("[MvcTracerFilter]:[OnResultExecuting]: {0}/{1}", filterContext.Controller, filterContext.Result);
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            Trace.TraceInformation("[MvcTracerFilter]:[OnResultExecuted]: {0}/{1}", filterContext.Controller, filterContext.Result);
        }
    }
}