using System;
using System.Data.Entity.Validation;
using System.Text;
using System.Web;
using Newtonsoft.Json;

namespace MvcLib.Common.Mvc
{
    public static class ResponseExtensions
    {
        public static void WriteAjaxException(this HttpResponseBase response, Exception ex)
        {
            var vex = ex as DbEntityValidationException;
            if (vex != null)
            {
                var msg = new StringBuilder();
                foreach (var validationError in vex.EntityValidationErrors)
                {
                    foreach (var error in validationError.ValidationErrors)
                    {
                        msg.AppendLine(string.Format("{0}: {1}", error.PropertyName, error.ErrorMessage));
                    }
                }

                response.WriteAjax(new
                {
                    success = false,
                    msg = msg.ToString()
                }, true, 400);

                return;
            }

            var sb = new StringBuilder();
            var exc = ex;
            do
            {
                sb.AppendLine(string.Format("[{0}]: {1}\r\n", exc.GetType(), exc.Message));
                if (exc.InnerException != null)
                    exc = exc.InnerException;
                else break;
            } while (true);

            response.WriteAjax(new
            {
                success = false,
                msg = sb.ToString()
            }, true, 500);

        }

        public static void WriteAjax<T>(this HttpResponseBase response, T value)
        {
            response.Clear();
            response.ContentType = "application/json";
            response.StatusCode = 200;
            SetNoCache(response);

            var settings = new JsonSerializerSettings()
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                FloatParseHandling = FloatParseHandling.Decimal
            };

            var json = JsonConvert.SerializeObject(value, settings);

            response.Write(json);
        }

        public static void WriteAjax(this HttpResponseBase response, object obj, bool setNoCache = true, int status = 200, JsonSerializerSettings settings = null)
        {
            response.Clear();
            response.ContentType = "application/json";
            response.StatusCode = status;
            if (setNoCache)
                SetNoCache(response);

            settings = settings ?? new JsonSerializerSettings()
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                FloatParseHandling = FloatParseHandling.Decimal
            };

            var json = JsonConvert.SerializeObject(obj, settings);

            response.Write(json);
        }

        public static void WriteAjax(this HttpResponseBase response, bool success = true, string msg = "", int status = 200, bool noCache = true)
        {
            WriteAjax(response, new
            {
                success,
                msg
            }, noCache, status);
        }

        public static void SetNoCache(this HttpResponseBase response)
        {
            response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
            response.Cache.SetValidUntilExpires(false);
            response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            response.Cache.SetCacheability(HttpCacheability.NoCache);
            response.Cache.SetNoStore();
        }

        public static void SetNoCache(this HttpResponse response)
        {
            response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
            response.Cache.SetValidUntilExpires(false);
            response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            response.Cache.SetCacheability(HttpCacheability.NoCache);
            response.Cache.SetNoStore();
        }
    }
}