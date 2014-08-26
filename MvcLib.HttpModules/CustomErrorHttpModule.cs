using System;
using System.Diagnostics;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using MvcLib.Common;
using MvcLib.Common.Mvc;

namespace MvcFromDb.Infra
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

            try
            {
                var html = ViewRenderer.RenderView(_errorViewPath, model);
                response.Write(html);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
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
                Trace.TraceError("Error executing controller {0}: {1}", controller, ex.Message);
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