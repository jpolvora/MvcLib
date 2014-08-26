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
    }
}