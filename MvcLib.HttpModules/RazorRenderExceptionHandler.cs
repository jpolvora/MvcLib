using System;
using System.Configuration;
using System.Diagnostics;
using System.Net.Mail;
using System.Threading;
using System.Web;
using MvcLib.Common;
using MvcLib.Common.Configuration;
using MvcLib.Common.Mvc;

namespace MvcLib.HttpModules
{
    public class RazorRenderExceptionHandler : ExceptionHandler<CustomException>
    {
        public RazorRenderExceptionHandler(HttpApplication application, string errorViewPath)
            : base(application, errorViewPath, LogActionEx)
        {
        }

        static void LogActionEx(HttpException exception)
        {
            var status = exception.GetHttpCode();
            if (status < 500) return;
            LogEvent.Raise(exception.Message, exception.GetBaseException());

            var cfg = BootstrapperSection.Instance;

            if (cfg.Mail.SendExceptionToDeveloper)
            {
                Trace.TraceInformation("[RazorRenderExceptionHandler]: Preparing to send email to developer");
                string body = exception.GetHtmlErrorMessage();
                string subject = exception.Message;
                ThreadPool.QueueUserWorkItem(x =>
                {
                    try
                    {
                        using (var client = new SmtpClient())
                        {
                            var msg = new MailMessage(cfg.Mail.MailAdmin, cfg.Mail.MailDeveloper, subject, body)
                            {
                                IsBodyHtml = true
                            };

                            client.Send(msg);
                            Trace.TraceInformation("[RazorRenderExceptionHandler]: Email was sent to {0}", cfg.Mail.MailDeveloper);
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("[RazorRenderExceptionHandler]: Failed to send email. {0}", ex.Message);
                    }
                });

            }
        }

        protected override bool IsProduction()
        {
            //checa se o ambiente é de produção
            bool release = ConfigurationManager.AppSettings["Environment"]
                .Equals("Release", StringComparison.OrdinalIgnoreCase);

            return release;
        }

        protected override void RenderCustomException(CustomException exception)
        {
            //Application.Context.RewritePath(ErrorViewPath);

            Trace.TraceInformation("[RazorRenderExceptionHandler]: Rendering Custom Exception");
            var model = new ErrorModel()
            {
                Message = exception.Message,
                FullMessage = exception.ToString(),
                StackTrace = exception.StackTrace,
                Url = Application.Request.RawUrl,
                StatusCode = Application.Response.StatusCode,
            };

            Trace.TraceWarning("Rendering razor view: {0}", ErrorViewPath);
            var html = ViewRenderer.RenderView(ErrorViewPath, model);
            Application.Response.Write(html);

        }
    }

    public class ErrorModel
    {
        public string Message { get; set; }
        public string FullMessage { get; set; }
        public string StackTrace { get; set; }
        public string Url { get; set; }
        public int StatusCode { get; set; }
        public string StatusDescription { get; set; }

        public override string ToString()
        {
            return string.Format("{0} - {1}", Message, Url);
        }
    }
}