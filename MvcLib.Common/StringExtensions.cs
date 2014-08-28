using System.Linq;

namespace MvcLib.Common
{
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
    }
}