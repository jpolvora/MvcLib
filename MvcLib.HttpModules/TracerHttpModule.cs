using System;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using MvcLib.Common;
using MvcLib.Common.Configuration;
using MvcLib.Common.Mvc;

namespace MvcLib.HttpModules
{
    public class TracerHttpModule : IHttpModule
    {
        private static int _counter;
        private const string RequestId = "_request:id";

        private const string Stopwatch = "__StopWatch";

        //private HttpApplication _application;

        public void Dispose()
        {
            Trace.TraceInformation("Dispose: {0}", this);
        }

        public override string ToString()
        {
            return string.Format("{0} = {1}", _counter, base.ToString());
        }

        private static string[] _eventsToTrace = new string[0];

        static bool MustLog(string eventName)
        {
            if (_eventsToTrace.Length == 0)
                return true;

            if (_eventsToTrace.Any(x => x.IsEmpty()))
                return true;

            return _eventsToTrace.Any(x => x.Equals(eventName, StringComparison.OrdinalIgnoreCase));
        }

        public void Init(HttpApplication on)
        {
            _eventsToTrace = BootstrapperSection.Instance.HttpModules.Trace.Events.Split(',');

            _counter++;
            Trace.TraceInformation("Init: {0}", this);

            //http://msdn.microsoft.com/en-us/library/bb470252(v=vs.100).aspx

            /*
             * The following tasks are performed by the HttpApplication class while the request is being processed. The events are useful for page developers who want to run code when key request pipeline events are raised. They are also useful if you are developing a custom module and you want the module to be invoked for all requests to the pipeline. Custom modules implement the IHttpModule interface. In Integrated mode in IIS 7.0, you must register event handlers in a module's Init method.
        Validate the request, which examines the information sent by the browser and determines whether it contains potentially malicious markup. For more information, see ValidateRequest and Script Exploits Overview.
        Perform URL mapping, if any URLs have been configured in the UrlMappingsSection section of the Web.config file.
             */


            on.Error += (sender, args) => OnError(on);

            on.BeginRequest += (sender, args) => OnBeginRequest(on);
            on.AuthenticateRequest += (sender, args) => TraceNotification(on, "AuthenticateRequest");
            on.PostAuthenticateRequest += (sender, args) => TraceNotification(on, "PostAuthenticateRequest");
            on.AuthorizeRequest += (sender, args) => TraceNotification(on, "AuthorizeRequest");
            on.PostAuthorizeRequest += (sender, args) => TraceNotification(on, "PostAuthorizeRequest");
            on.ResolveRequestCache += (sender, args) => TraceNotification(on, "ResolveRequestCache");

            //MVC Routing module remaps the handler here.
            on.PostResolveRequestCache += (sender, args) => TraceNotification(on, "PostResolveRequestCache");

            //only iis7
            on.MapRequestHandler += (sender, args) => TraceNotification(on, "MapRequestHandler");

            //An appropriate handler is selected based on the file-name extension of the requested resource. The handler can be a native-code module such as the IIS 7.0 StaticFileModule or a managed-code module such as the PageHandlerFactory class (which handles .aspx files). 
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

            //The MapRequestHandler, LogRequest, and PostLogRequest events are supported only if the application is running in Integrated mode in IIS 7.0 and with the .NET Framework 3.0 or later.
            on.LogRequest += (sender, args) => TraceNotification(on, "LogRequest"); //iis7
            on.PostLogRequest += (sender, args) => TraceNotification(on, "PostLogRequest"); //iis7
            on.EndRequest += (sender, args) => OnEndRequest(on);
            on.PreSendRequestHeaders += (sender, args) => TraceNotification(on, "PreSendRequestHeaders");
            on.PreSendRequestContent += (sender, args) => TraceNotification(on, "PreSendRequestContent");
        }

        private void TraceNotification(HttpApplication application, string eventName)
        {
            if (!MustLog(eventName))
                return;

            var rid = application.Context.Items[RequestId];
            Trace.TraceInformation("[{0}]:[{1}] Evento {2}, Handler: [{3}], Filter: {4}, User: {5}, Memory: {6}",
                application.Context.CurrentNotification, rid, eventName, application.Context.CurrentHandler,
                application.Context.Response.Filter, application.User != null ? application.User.Identity.Name : "-",
                GC.GetTotalMemory(false));

            //case RequestNotification.PreExecuteRequestHandler:
            if (RequestNotification.PreExecuteRequestHandler == application.Context.CurrentNotification)
            {
                var mvcHandler = application.Context.Handler as MvcHandler;
                if (mvcHandler != null)
                {
                    var controller = mvcHandler.RequestContext.RouteData.GetRequiredString("controller");
                    var action = mvcHandler.RequestContext.RouteData.GetRequiredString("action");
                    var area = mvcHandler.RequestContext.RouteData.DataTokens["area"];

                    Trace.TraceInformation("Entering MVC Pipeline. Area: '{0}', Controller: '{1}', Action: '{2}'", area,
                        controller, action);
                }
            }
        }

        private void OnBeginRequest(HttpApplication application)
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

            if (context.Items.Contains("IIS_WasUrlRewritten"))
            {
                Trace.TraceInformation("Url was rewriten from '{0}' to '{1}'", context.Request.ServerVariables["HTTP_X_ORIGINAL_URL"], context.Request.ServerVariables["SCRIPT_NAME"]);
            }

            Trace.TraceInformation("[BeginRequest]:[{0}] {1} {2} {3}", rid, context.Request.HttpMethod, context.Request.RawUrl, isAjax ? "Ajax: True" : "");
        }

        private void OnEndRequest(HttpApplication application)
        {
            StopTimer(application);
            Trace.Flush();

            var context = application.Context;
            var handler = context.Handler;

            var rid = context.Items[RequestId];

            var msg = string.Format("[EndRequest]:[{0}] handler: {1}, Content-Type: {2}, Status: {3}, Render: {4}, url: {5}", rid, handler, context.Response.ContentType, context.Response.StatusCode, GetTime(application), context.Request.RawUrl);
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

        private void OnError(HttpApplication application)
        {
            if (BootstrapperSection.Instance.StopMonitoring)
            StopTimer(application);
            Trace.Flush();

            var rid = application.Context.Items[RequestId];
            Trace.TraceInformation("[{0}]:[{1}] Evento {2}, Handler: [{3}], User: {4}", application.Context.CurrentNotification, rid, "Error: ", application.Context.CurrentHandler, application.User != null ? application.User.Identity.Name : "-");
        }

        private static void StopTimer(HttpApplication _application)
        {
            if (_application == null || _application.Context == null) return;
            var stopwatch = _application.Context.Items[Stopwatch] as Stopwatch;
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