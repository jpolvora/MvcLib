using System;
using System.Diagnostics;
using System.Web;
using System.Web.WebPages;

namespace MvcLib.HttpModules
{
    public class ExceptionHandler<TException> : IDisposable
        where TException : Exception
    {
        protected readonly HttpApplication Application;
        protected readonly string ErrorViewPath;
        protected readonly Action<HttpException> LogAction;

        public ExceptionHandler(HttpApplication application, string errorViewPath)
            : this(application, errorViewPath, exception => Trace.TraceError(exception.Message))
        {
        }

        public ExceptionHandler(HttpApplication application, string errorViewPath, Action<HttpException> logAction)
        {
            Application = application;
            ErrorViewPath = errorViewPath;
            LogAction = logAction;
        }

        public virtual void HandleError()
        {
            var server = Application.Server;
            var response = Application.Response;

            Exception ex = server.GetLastError();

            HttpException httpException = ex as HttpException
                ?? new HttpException("Generic exception...", ex);

            var rootException = httpException.GetBaseException();

            Trace.TraceError("[ExceptionHandler]: {0}", rootException.Message);

            if (IsProduction())
            {
                //log or send email to developer notifiying the exception ?
                LogAction(httpException);
                server.ClearError(); //limpar o erro para exibir a página customizada
            }

            var statusCode = httpException.GetHttpCode();

            //setar o statuscode para que o IIS selecione a view correta (no web.config)
            response.StatusCode = statusCode;
            response.StatusDescription = rootException.Message; //todo: colocar uma msg melhor

            switch (statusCode)
            {
                case 404:
                    break; //IIS will handle 404
                case 500:
                    {
                        //check for exception type you want to show custom message
                        if (rootException is TException)
                        {
                            server.ClearError();
                            response.TrySkipIisCustomErrors = true;
                            response.Clear();

                            try
                            {
                                RenderCustomException(rootException as TException);
                            }
                            catch
                            {
                                //fallback to response.Write
                                response.Write(rootException.ToString());
                            }
                        }
                        break; //IIS will handle 500
                    }
            }
        }

        /// <summary>
        /// retorna true se app está em produção
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsProduction()
        {
            return Application.Context.IsCustomErrorEnabled;
        }

        /// <summary>
        /// Overridable Method. Default implementation uses Razor WebPages.
        /// </summary>
        /// <param name="exception"></param>
        protected virtual void RenderCustomException(TException exception)
        {
            //stores exception in session for later retrieve
            Application.Session["exception"] = exception;

            //executa a página
            var handler = WebPageHttpHandler.CreateFromVirtualPath(ErrorViewPath);
            handler.ProcessRequest(Application.Context);

            Application.Session.Remove("exception");
        }

        public virtual void Dispose()
        {
            Application.CompleteRequest();
        }
    }
}