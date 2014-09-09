using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcLib.Common.Mvc
{
    public static class WebHelpers
    {
        public static bool IsLocalUrl(this string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }
            return ((url[0] == '/' && (url.Length == 1 ||
                                       (url[1] != '/' && url[1] != '\\'))) ||   // "/" or "/foo" but not "//" or "/\"
                    (url.Length > 1 &&
                     url[0] == '~' && url[1] == '/'));   // "~/" or "~/foo"
        }
    }
}
