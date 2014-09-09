using System;
using System.Web;
using System.Web.Helpers;
using System.Web.WebPages;

namespace MvcLib.Common.Mvc
{
    public static class RequestExtensions
    {
        public static bool IsAjaxRequest(this HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            return (request["X-Requested-With"] == "XMLHttpRequest") || ((request.Headers != null) && (request.Headers["X-Requested-With"] == "XMLHttpRequest"));
        }

        public static string ToPublicUrl(this HttpContextBase context, Uri relativeUri, string scheme)
        {
            
            var uriBuilder = new UriBuilder
            {
                Host = context.Request.Url.Host,
                Path = "/",
                Port = 80,
                Scheme = scheme,
            };

            if (context.Request.IsLocal)
            {
                uriBuilder.Port = context.Request.Url.Port;
            }

            return new Uri(uriBuilder.Uri, relativeUri).AbsoluteUri;
        }

        public static void ValidateAntiforgeryFromHeader(this HttpRequestBase request, int statusCode = 0)
        {
            //ajax setup
            /*
                var token = $('input[name="__RequestVerificationToken"]').val();

                $.ajaxSetup({
                    beforeSend: function (xhr) {
                        //console.log(xhr);
                        xhr.setRequestHeader("token", token);
                    }
                });
             * 
             */
            var antiForgeryCookie = request.Cookies[AntiForgeryConfig.CookieName];

            var cookieValue = antiForgeryCookie != null
                ? antiForgeryCookie.Value
                : null;
            try
            {
                AntiForgery.Validate(cookieValue, request.Headers["token"]);
            }
            catch (Exception ex)
            {
                if (statusCode == 0)
                    throw;

                request.RequestContext.HttpContext.Response.SetStatus(statusCode);
            }
        }
    }
}