using System;
using System.Diagnostics;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using MvcLib.Common;
using MvcLib.Common.Mvc;

namespace MvcLib.HttpModules
{
    public class CustomErrorHttpModule : IHttpModule
    {
        private string _errorViewPath;
        private string _errorController;

        public void Init(HttpApplication context)
        {
            context.BeginRequest += OnBeginRequest;
            context.Error += OnError;

            _errorViewPath = Config.ValueOrDefault("CustomErrorViewPath", "~/views/shared/customerror.cshtml");
            _errorController = Config.ValueOrDefault("CustomErrorController", "");
        }

        static void OnBeginRequest(object sender, EventArgs eventArgs)
        {
            var application = (HttpApplication)sender;
            application.Response.TrySkipIisCustomErrors = true;
        }

        void OnError(object sender, EventArgs args)
        {
            var application = (HttpApplication)sender;

            var server = application.Server;
            var response = application.Response;
            var exception = server.GetLastError();

            server.ClearError();
            response.Clear();

            var model = new ErrorModel()
            {
                Message = exception != null ? exception.Message : "Erro: " + response.Status,
                StackTrace = exception != null ? exception.StackTrace : "",
                Url = application.Request.RawUrl,
                StatusCode = response.StatusCode
            };

            bool useController = !string.IsNullOrWhiteSpace(_errorController);
            if (useController)
            {
                RenderController(application.Context, _errorController, model);
            }
            else
            {
                RenderView(_errorViewPath, model, response);
            }

            Trace.TraceInformation("[CustomError]: Will end response now (avoid Transfer Handler)");
            response.End();
        }

        private static void RenderView(string errorViewPath, ErrorModel model, HttpResponse response)
        {
            try
            {
                var html = ViewRenderer.RenderView(errorViewPath, model);
                response.Write(html);
                
            }
            catch (Exception ex)
            {
                var msg = string.Format("Error rendering view '{0}': {1}", errorViewPath, ex.Message);
                response.Write(msg);
                Trace.TraceError(msg);
            }
        }

        private static void RenderController(HttpContext context, string controllerName, ErrorModel model)
        {
            var wrapper = new HttpContextWrapper(context);

            var routeData = new RouteData();
            routeData.Values.Add("controller", controllerName);
            routeData.Values.Add("Message", model.Message);
            routeData.Values.Add("StackTrace", model.StackTrace);
            routeData.Values.Add("Url", model.Url);
            routeData.Values.Add("StatusCode", model.StatusCode);

            var factory = ControllerBuilder.Current.GetControllerFactory();

            IController controller = null;
            try
            {
                var requestContext = new RequestContext(wrapper, routeData);
                controller = factory.CreateController(requestContext, controllerName);
                if (controller != null)
                {
                    
                    controller.Execute(requestContext);
                }
            }
            catch (Exception ex)
            {
                var msg = string.Format("Error executing controller {0}: {1}", controller, ex.Message);
                context.Response.Write(msg);
                Trace.TraceError(msg);
            }
            finally
            {
                if (controller != null)
                {
                    factory.ReleaseController(controller);
                }
            }
        }

        public void Dispose()
        {
        }

        public class ErrorModel
        {
            public string Message { get; set; }
            public string StackTrace { get; set; }
            public string Url { get; set; }
            public int StatusCode { get; set; }

            public override string ToString()
            {
                return string.Format("{0} - {1}", Message, Url);
            }
        }
    }
}