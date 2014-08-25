using System;
using System.Diagnostics;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MvcFromDb.Infra
{
    //http://msdn.microsoft.com/en-us/library/bb470252(v=vs.100).aspx

    /*
     * The following tasks are performed by the HttpApplication class while the request is being processed. The events are useful for page developers who want to run code when key request pipeline events are raised. They are also useful if you are developing a custom module and you want the module to be invoked for all requests to the pipeline. Custom modules implement the IHttpModule interface. In Integrated mode in IIS 7.0, you must register event handlers in a module's Init method.
Validate the request, which examines the information sent by the browser and determines whether it contains potentially malicious markup. For more information, see ValidateRequest and Script Exploits Overview.
Perform URL mapping, if any URLs have been configured in the UrlMappingsSection section of the Web.config file.
Raise the BeginRequest event.
Raise the AuthenticateRequest event.
Raise the PostAuthenticateRequest event.
Raise the AuthorizeRequest event.
Raise the PostAuthorizeRequest event.
Raise the ResolveRequestCache event.
Raise the PostResolveRequestCache event.
Raise the MapRequestHandler event. An appropriate handler is selected based on the file-name extension of the requested resource. The handler can be a native-code module such as the IIS 7.0 StaticFileModule or a managed-code module such as the PageHandlerFactory class (which handles .aspx files). 
Raise the PostMapRequestHandler event.
Raise the AcquireRequestState event.
Raise the PostAcquireRequestState event.
Raise the PreRequestHandlerExecute event.
Call the ProcessRequest method (or the asynchronous version IHttpAsyncHandler.BeginProcessRequest) of the appropriate IHttpHandler class for the request. For example, if the request is for a page, the current page instance handles the request.
Raise the PostRequestHandlerExecute event.
Raise the ReleaseRequestState event.
Raise the PostReleaseRequestState event.
Perform response filtering if the Filter property is defined.
Raise the UpdateRequestCache event.
Raise the PostUpdateRequestCache event.
Raise the LogRequest event.
Raise the PostLogRequest event.
Raise the EndRequest event.
Raise the PreSendRequestHeaders event.
Raise the PreSendRequestContent event.
NoteNote	The MapRequestHandler, LogRequest, and PostLogRequest events are supported only if the application is running in Integrated mode in IIS 7.0 and with the .NET Framework 3.0 or later.
     */
    public class TracerHttpModule : IHttpModule
    {
        private static int counter;
        private const string REQUEST_ID = "_request:id";

        private const string STOPWATCH = "__StopWatch";

        private HttpApplication _application;

        public void Dispose()
        {
            StopTimer();
            Trace.TraceInformation("Dispose: {0}", this);
        }

        public override string ToString()
        {
            return string.Format("{0} = {1}", counter, base.ToString());
        }

        public void Init(HttpApplication on)
        {
            counter++;
            Trace.TraceInformation("Init: {0}", this);

            _application = on;

            on.Error += OnError;
            on.BeginRequest += OnBeginRequest;
            on.AuthenticateRequest += (sender, args) => LogNotification(sender, "AuthenticateRequest");
            on.PostAuthenticateRequest += (sender, args) => LogNotification(sender, "PostAuthenticateRequest");
            on.AuthorizeRequest += (sender, args) => LogNotification(sender, "AuthorizeRequest");
            on.PostAuthorizeRequest += (sender, args) => LogNotification(sender, "PostAuthorizeRequest");
            on.ResolveRequestCache += (sender, args) => LogNotification(sender, "ResolveRequestCache");
            on.PostResolveRequestCache += (sender, args) => LogNotification(sender, "PostResolveRequestCache"); //MVC Routing module remaps the handler here.
            on.MapRequestHandler += (sender, args) => LogNotification(sender, "MapRequestHandler");  //only iis7
            on.PostMapRequestHandler += (sender, args) => LogNotification(sender, "PostMapRequestHandler");
            on.AcquireRequestState += (sender, args) => LogNotification(sender, "AcquireRequestState");
            on.PostAcquireRequestState += (sender, args) => LogNotification(sender, "PostAcquireRequestState");
            on.PreRequestHandlerExecute += (sender, args) => LogNotification(sender, "PreRequestHandlerExecute");
            //Call the ProcessRequest of IHttpHandler
            on.PostRequestHandlerExecute += (sender, args) => LogNotification(sender, "PostRequestHandlerExecute");
            on.ReleaseRequestState += (sender, args) => LogNotification(sender, "ReleaseRequestState");
            on.PostReleaseRequestState += (sender, args) => LogNotification(sender, "PostReleaseRequestState"); //Perform response filtering if the Filter property is defined.
            on.UpdateRequestCache += (sender, args) => LogNotification(sender, "UpdateRequestCache");
            on.PostUpdateRequestCache += (sender, args) => LogNotification(sender, "PostUpdateRequestCache");
            on.LogRequest += (sender, args) => LogNotification(sender, "LogRequest"); //iis7
            on.PostLogRequest += (sender, args) => LogNotification(sender, "PostLogRequest"); //iis7
            on.EndRequest += OnEndRequest;
            on.PreSendRequestHeaders += (sender, args) => LogNotification(sender, "PreSendRequestHeaders");
            on.PreSendRequestContent += (sender, args) => LogNotification(sender, "PreSendRequestContent");
        }

        private void LogNotification(object sender, string eventName)
        {
            var rid = _application.Context.Items[REQUEST_ID];
            Trace.TraceInformation("[{0}]:[{1}] Evento {2}, Handler: [{3}], User: {4}", _application.Context.CurrentNotification, rid, eventName, _application.Context.CurrentHandler, _application.User != null ? _application.User.Identity.Name : "-");

            switch (_application.Context.CurrentNotification)
            {
                case RequestNotification.PreExecuteRequestHandler:
                    {
                        var mvcHandler = _application.Context.Handler as MvcHandler;
                        if (mvcHandler != null)
                        {
                            var handler = mvcHandler;
                            var controller = handler.RequestContext.RouteData.GetRequiredString("controller");
                            var action = handler.RequestContext.RouteData.GetRequiredString("action");
                            var area = handler.RequestContext.RouteData.DataTokens["area"];
                            Trace.TraceInformation("Entering MVC Pipeline. Area: {0}, Controller: {1}, Action: {2}", area,
                                controller, action);
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        private void OnBeginRequest(object sender, EventArgs eventArgs)
        {
            var context = _application.Context;

            var rid = new Random().Next(1, 99999).ToString("d5");
            context.Items.Add(REQUEST_ID, rid);

            context.Items[STOPWATCH] = Stopwatch.StartNew();

            bool isAjax = new HttpContextWrapper(context).Request.IsAjaxRequest();

            if (isAjax)
            {
                context.Response.SuppressFormsAuthenticationRedirect = true;
            }

            Trace.TraceInformation("[BeginRequest]:[{0}] {1} {2} {3}", rid, context.Request.HttpMethod, context.Request.RawUrl, isAjax ? "Ajax: True" : "");
        }

        private void OnEndRequest(object sender, EventArgs e)
        {
            StopTimer();

            var context = _application.Context;
            var handler = context.Handler;

            var rid = context.Items[REQUEST_ID];

            var msg = string.Format("[EndRequest]:[{0}] handler: {1}, Content-Type: {2}, Status: {3}, Render: {4}, url: {5}", rid, handler, context.Response.ContentType, context.Response.StatusCode, GetTime(), context.Request.RawUrl);
            Trace.TraceInformation(msg);

            if (context.Request.IsAuthenticated && context.Response.StatusCode == 403)
            {
                bool isAjax = new HttpContextWrapper(context).Request.IsAjaxRequest();
                if (!isAjax)
                {
                    context.Response.Write("Você está autenticado mas não possui permissões para acessar este recurso");
                }
            }
        }

        private void OnError(object sender, EventArgs e)
        {
            StopTimer();

            var server = _application.Server;
            var context = _application.Context;
            var request = _application.Request;
            var response = _application.Response;

            var wrapper = new HttpContextWrapper(context);
            Exception exception = server.GetLastError() ?? new Exception("(GetLastError was null") { Source = _application.ToString() };

            server.ClearError();

            response.TrySkipIisCustomErrors = true;
            Trace.TraceError("[Application_Error]: {0} - {1}", exception.Source, exception);

            if (wrapper.Request.IsAjaxRequest())
            {
                response.ContentType = "application/json";
                response.StatusCode = exception is HttpException ? ((HttpException)exception).GetHttpCode() : 500;
                response.StatusDescription = exception.Message;
                response.End();
                return;
            }

            var routeData = new RouteData();
            routeData.Values.Add("controller", "Error");
            routeData.Values.Add("exception", exception);

            if (exception.GetType() == typeof(HttpException))
            {
                var statusCode = ((HttpException)exception).GetHttpCode();
                routeData.Values.Add("statusCode", statusCode);
                if (statusCode == 404)
                {
                    routeData.Values.Add("action", "NotFound");
                    routeData.Values.Add("url", request.Url);
                }
                else
                {
                    routeData.Values.Add("action", "Index");
                }
            }
            else
            {
                routeData.Values.Add("statusCode", 500);
                routeData.Values.Add("action", "Index");
            }

            var requestContext = new RequestContext(wrapper, routeData);

            var factory = ControllerBuilder.Current.GetControllerFactory();

            IController controller = null;
            try
            {
                controller = factory.CreateController(requestContext, "Error");
                if (controller != null)
                {
                    controller.Execute(requestContext);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error executing controller {0}: {1}", controller, ex.Message);
            }
            finally
            {
                if (controller != null)
                {
                    factory.ReleaseController(controller);
                }
            }

            response.End();
        }

        private void StopTimer()
        {
            if (_application == null || _application.Context == null) return;
            var stopwatch = _application.Context.Items[STOPWATCH] as Stopwatch;
            if (stopwatch != null)
                stopwatch.Stop();
        }

        private double GetTime()
        {
            if (_application == null || _application.Context == null) return -1;

            var stopwatch = _application.Context.Items[STOPWATCH] as Stopwatch;
            if (stopwatch != null)
            {
                var ts = stopwatch.Elapsed.TotalSeconds;
                return ts;
            }
            return -1;
        }
    }
}