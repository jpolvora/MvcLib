using System;
using System.Web;
using MvcLib.Common.Configuration;

namespace MvcLib.HttpModules
{
    public class CustomErrorHttpModule : IHttpModule
    {
        private string _errorViewPath;

        public void Init(HttpApplication context)
        {
            context.BeginRequest += OnBeginRequest;
            context.Error += OnError;

            _errorViewPath = BootstrapperSection.Instance.HttpModules.CustomError.ErrorViewPath;
        }

        static void OnBeginRequest(object sender, EventArgs eventArgs)
        {
        }

        void OnError(object sender, EventArgs args)
        {
            //<httpErrors errorMode="Custom" existingResponse="Auto" defaultResponseMode="ExecuteURL">
            //  <remove statusCode="404"/>
            //  <error statusCode="404" path="/404.cshtml" responseMode="ExecuteURL"/>
            //  <remove statusCode="500"/>
            //  <error statusCode="500" path="/500.cshtml" responseMode="ExecuteURL"/>
            //</httpErrors>

            using (var razorhelper = new RazorRenderExceptionHandler(sender as HttpApplication, _errorViewPath))
            {
                razorhelper.HandleError();
            }
        }

        public void Dispose()
        {
        }


    }
}