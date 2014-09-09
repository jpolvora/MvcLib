using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MvcLib.Common
{

    public static class ArrayExtensions
    {
        public static T[] Extend<T>(this T[] srcArray, bool insertAfter = true, params T[] addItems) where T : class
        {
            if (addItems == null)
            {
                throw new ArgumentNullException("addItems");
            }
            if (srcArray == null)
            {
                return addItems;
            }

            var result = insertAfter
                ? srcArray.Concat(addItems).ToArray()
                : addItems.Concat(srcArray).ToArray();

            srcArray = result;

            return srcArray;
        }
    }


    public static class StringExtensions
    {
        public static string Fmt(this string str, params object[] args)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            return string.Format(str, args);
        }

        public static string Truncate(this string str, int size, bool trim = false)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;
            if (str.Length > size)
                return new string(str.Take(size).ToArray());

            return trim ? str.Trim() : str;
        }

        public static bool IsNotNullOrWhiteSpace(this string str)
        {
            return !string.IsNullOrWhiteSpace(str);
        }

        public static string RemapInternationalCharToAscii(char c)
        {
            string s = c.ToString().ToLowerInvariant();
            if ("àåáâäãåą".Contains(s))
            {
                return "a";
            }
            if ("&èéêëę".Contains(s))
            {
                return "e";
            }
            if ("ìíîïı".Contains(s))
            {
                return "i";
            }
            if ("òóôõöøőð".Contains(s))
            {
                return "o";
            }
            if ("ùúûüŭů".Contains(s))
            {
                return "u";
            }
            if ("çćčĉ".Contains(s))
            {
                return "c";
            }
            if ("żźž".Contains(s))
            {
                return "z";
            }
            if ("śşšŝ".Contains(s))
            {
                return "s";
            }
            if ("ñń".Contains(s))
            {
                return "n";
            }
            if ("ýÿ".Contains(s))
            {
                return "y";
            }
            if ("ğĝ".Contains(s))
            {
                return "g";
            }
            if (c == 'ř')
            {
                return "r";
            }
            if (c == 'ł')
            {
                return "l";
            }
            if (c == 'đ')
            {
                return "d";
            }
            if (c == 'ß')
            {
                return "ss";
            }
            if (c == 'Þ')
            {
                return "th";
            }
            if (c == 'ĥ')
            {
                return "h";
            }
            if (c == 'ĵ')
            {
                return "j";
            }
            return "";
        }

        /// <summary>
        /// Produces optional, URL-friendly version of a title, "like-this-one". 
        /// hand-tuned for speed, reflects performance refactoring contributed
        /// by John Gietzen (user otac0n) 
        /// </summary>
        public static string URLFriendly(this string title)
        {
            if (title == null) return "";

            const int maxlen = 80;
            int len = title.Length;
            bool prevdash = false;
            var sb = new StringBuilder(len);

            for (int i = 0; i < len; i++)
            {
                var c = title[i];
                if (c == '&')
                {
                    sb.Append('e');
                    if (i < len - 1)
                        sb.Append('-');
                }
                else if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                {
                    sb.Append(c);
                    prevdash = false;
                }
                else if (c >= 'A' && c <= 'Z')
                {
                    // tricky way to convert to lowercase
                    sb.Append((char)(c | 32));
                    prevdash = false;
                }
                else if (c == ' ' || c == ',' || c == '.' || c == '/' ||
                    c == '\\' || c == '-' || c == '_' || c == '=')
                {
                    if (!prevdash && sb.Length > 0)
                    {
                        sb.Append('-');
                        prevdash = true;
                    }
                }
                else if (c >= 128)
                {
                    int prevlen = sb.Length;
                    sb.Append(RemapInternationalCharToAscii(c));
                    if (prevlen != sb.Length) prevdash = false;
                }
                if (i == maxlen) break;
            }

            return prevdash ? sb.ToString().Substring(0, sb.Length - 1) : sb.ToString();
        }

        public static string ToDebugString<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            return "{" + string.Join(",", dictionary.Select(kv => kv.Key + "=" + kv.Value).ToArray()) + "}";
        }

    }
}