using System;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using MvcLib.Common.Configuration;
using MvcLib.Common.Mvc;

namespace MvcLib.HttpModules
{
    public class TracerHttpModule : IHttpModule
    {
        private const string RequestId = "_request:id";

        private const string Stopwatch = "_request:sw";

        
        public void Dispose()
        {
        }

        static TracerHttpModule()
        {
            EventsToTrace = BootstrapperSection.Instance.HttpModules.Trace.Events.Split(',');
        }

        private static readonly string[] EventsToTrace = new string[0];

        static bool MustLog(string eventName)
        {
            if (EventsToTrace.Length == 0)
                return true;

            if (EventsToTrace.Any(x => x.IsEmpty()))
                return true;

            foreach (var x in EventsToTrace)
            {
                if (x.Equals(eventName, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        public void Init(HttpApplication on)
        {
            //http://msdn.microsoft.com/en-us/library/bb470252(v=vs.100).aspx

            on.Error += (sender, args) => OnError(sender);

            on.BeginRequest += (sender, args) => TraceNotification(on, "BeginRequest");
            on.AuthenticateRequest += (sender, args) => TraceNotification(on, "AuthenticateRequest");
            on.PostAuthenticateRequest += (sender, args) => TraceNotification(on, "PostAuthenticateRequest");
            on.AuthorizeRequest += (sender, args) => TraceNotification(on, "AuthorizeRequest");
            on.PostAuthorizeRequest += (sender, args) => TraceNotification(on, "PostAuthorizeRequest");
            on.ResolveRequestCache += (sender, args) => TraceNotification(on, "ResolveRequestCache");

            //MVC Routing module remaps the handler here.
            on.PostResolveRequestCache += (sender, args) => TraceNotification(on, "PostResolveRequestCache");

            //only iis7
            on.MapRequestHandler += (sender, args) => TraceNotification(on, "MapRequestHandler");

            //An appropriate handler is selected based on the file-name extension of the requested resource. 
            //The handler can be a native-code module such as the IIS 7.0 StaticFileModule or a managed-code module
            //such as the PageHandlerFactory class (which handles .aspx files). 
            on.PostMapRequestHandler += (sender, args) => TraceNotification(on, "PostMapRequestHandler");

            on.AcquireRequestState += (sender, args) => TraceNotification(on, "AcquireRequestState");
            on.PostAcquireRequestState += (sender, args) => TraceNotification(on, "PostAcquireRequestState");
            on.PreRequestHandlerExecute += (sender, args) => TraceNotification(on, "PreRequestHandlerExecute");
            //Call the ProcessRequest of IHttpHandler
            on.PostRequestHandlerExecute += (sender, args) => TraceNotification(on, "PostRequestHandlerExecute");
            on.ReleaseRequestState += (sender, args) => TraceNotification(on, "ReleaseRequestState");

            //Perform response filtering if the Filter property is defined.
            on.PostReleaseRequestState += (sender, args) => TraceNotification(on, "PostReleaseRequestState");

            on.UpdateRequestCache += (sender, args) => TraceNotification(on, "UpdateRequestCache");
            on.PostUpdateRequestCache += (sender, args) => TraceNotification(on, "PostUpdateRequestCache");

            //The MapRequestHandler, LogRequest, and PostLogRequest events are supported only if the application 
            //is running in Integrated mode in IIS 7.0 and with the .NET Framework 3.0 or later.
            on.LogRequest += (sender, args) => TraceNotification(on, "LogRequest"); //iis7
            on.PostLogRequest += (sender, args) => TraceNotification(on, "PostLogRequest"); //iis7
            on.EndRequest += (sender, args) => TraceNotification(on, "EndRequest");
            on.PreSendRequestHeaders += (sender, args) => TraceNotification(on, "PreSendRequestHeaders");
            on.PreSendRequestContent += (sender, args) => TraceNotification(on, "PreSendRequestContent");
        }

        private static void TraceNotification(HttpApplication application, string eventName)
        {
            if (!MustLog(eventName))
                return;

            var rid = application.Context.Items[RequestId];
            Trace.TraceInformation("[TracerHttpModule]:{0}, rid: [{1}], [{2}], {3}",
                eventName, rid, application.Context.CurrentHandler,
                application.User != null ? application.User.Identity.Name : "-");

            switch (application.Context.CurrentNotification)
            {
                case RequestNotification.BeginRequest:
                    {
                        OnBeginRequest(application);
                        break;
                    }
                case RequestNotification.PreExecuteRequestHandler:
                    {
                        //will call ProcessRequest of IHttpHandler

                        var mvcHandler = application.Context.Handler as MvcHandler;
                        if (mvcHandler != null)
                        {
                            var controller = mvcHandler.RequestContext.RouteData.GetRequiredString("controller");
                            var action = mvcHandler.RequestContext.RouteData.GetRequiredString("action");
                            var area = mvcHandler.RequestContext.RouteData.DataTokens["area"];

                            Trace.TraceInformation(
                                "Entering MVC Pipeline. Area: '{0}', Controller: '{1}', Action: '{2}'", area,
                                controller, action);
                        }
                        else
                        {
                            Trace.TraceInformation("[TracerHttpModule]:Executing ProcessRequest of Handler {0}", application.Context.CurrentHandler);
                        }
                    }
                    break;
                case RequestNotification.ReleaseRequestState:
                    {
                        Trace.TraceInformation("[TracerHttpModule]:Response Filter: {0}", application.Context.Response.Filter);
                        break;
                    }
                case RequestNotification.EndRequest:
                    {
                        OnEndRequest(application);
                        break;
                    }
            }
        }

        private static void OnBeginRequest(HttpApplication application)
        {
            var context = application.Context;

            var rid = new Random().Next(1, 99999).ToString("d5");
            context.Items.Add(RequestId, rid);

            context.Items[Stopwatch] = System.Diagnostics.Stopwatch.StartNew();

            bool isAjax = context.Request.IsAjaxRequest();

            if (isAjax)
            {
                context.Response.SuppressFormsAuthenticationRedirect = true;
            }

            if (context.Items.Contains("IIS_WasUrlRewritten") || context.Items.Contains("HTTP_X_ORIGINAL_URL"))
            {
                Trace.TraceInformation("[TracerHttpModule]:Url was rewriten from '{0}' to '{1}'",
                    context.Request.ServerVariables["HTTP_X_ORIGINAL_URL"],
                    context.Request.ServerVariables["SCRIPT_NAME"]);
            }

            Trace.TraceInformation("[BeginRequest]:[{0}] {1} {2} {3}", rid, context.Request.HttpMethod,
                context.Request.RawUrl, isAjax ? "Ajax: True" : "");
        }

        private static void OnEndRequest(HttpApplication application)
        {
            StopTimer(application);
            Trace.Flush();

            var context = application.Context;

            var rid = context.Items[RequestId];

            var msg = string.Format("[EndRequest]:[{0}], Content-Type: {1}, Status: {2}, Render: {3}, url: {4}",
                rid, context.Response.ContentType, context.Response.StatusCode, GetTime(application),
                context.Request.Url);
            Trace.TraceInformation(msg);

            if (context.Request.IsAuthenticated && context.Response.StatusCode == 403)
            {
                bool isAjax = context.Request.IsAjaxRequest();
                if (!isAjax)
                {
                    context.Response.Write("Você está autenticado mas não possui permissões para acessar este recurso");
                }
            }
        }

        private static void OnError(object sender)
        {
            var application = (HttpApplication)sender;
            StopTimer(application);
            Trace.Flush();
            var rid = application.Context.Items[RequestId];
            Trace.TraceInformation("[TracerHttpModule]: Error at {0}, request {1}, Handler: {2}, Message:'{3}'",
                application.Context.CurrentNotification, rid, application.Context.CurrentHandler,
                application.Context.Error);
        }

        private static void StopTimer(HttpApplication application)
        {
            if (application == null || application.Context == null) return;
            var stopwatch = application.Context.Items[Stopwatch] as Stopwatch;
            if (stopwatch != null)
                stopwatch.Stop();
        }

        private static double GetTime(HttpApplication application)
        {
            if (application == null || application.Context == null) return -1;

            var stopwatch = application.Context.Items[Stopwatch] as Stopwatch;
            if (stopwatch != null)
            {
                var ts = stopwatch.Elapsed.TotalSeconds;
                return ts;
            }
            return -1;
        }
    }
}