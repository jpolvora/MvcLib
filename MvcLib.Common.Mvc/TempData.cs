using System.Collections.Generic;
using System.Web;

namespace MvcLib.Common.Mvc
{
    /// <summary>
    /// Requires Session
    /// </summary>
    public static class TempData
    {
        private const string Key = "_tempData";
        static TempData()
        {
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                HttpContext.Current.Session[Key] = new Queue<string>();
            }
        }

        public static string Get()
        {
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                var queue = HttpContext.Current.Session[Key] as Queue<string>;
                if (queue != null)
                {
                    return queue.Dequeue();
                }
            }

            return string.Empty;
        }

        public static bool HasItens()
        {
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                var queue = HttpContext.Current.Session[Key] as Queue<string>;
                if (queue != null)
                {
                    return queue.Count > 0;
                }
            }

            return false;
        }

        public static void Set(string value)
        {
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                var queue = HttpContext.Current.Session[Key] as Queue<string>;
                if (queue != null)
                {
                    queue.Enqueue(value);
                }

            }
        }
    }
}