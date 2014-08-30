using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MvcLib.Common
{
    public static class TypeUtils
    {
        public static T ValueOrDefault<T>(this T obj, T defaultValue)
        {
            var empty = default(T);

            return Equals(obj, empty) ? defaultValue : obj;
        }

        public static T As<T>(this object obj)
        {
            if (obj is T)
                return (T)obj;

            if (obj == null)
                return default(T);

            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter.CanConvertFrom(obj.GetType()))
            {
                try
                {
                    var result = converter.ConvertFrom(obj);
                    if (result is T)
                        return (T)result;
                }
                catch (Exception)
                {
                }
            }

            try
            {
                var result = (T)Convert.ChangeType(obj, typeof(T));
                return result;
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public static string AsString(this object obj)
        {
            return obj.As<string>();
        }

        public static int AsInt(this object obj)
        {
            if (obj is int)
                return (int)obj;

            var result = 0;

            if (obj != null)
                Int32.TryParse(obj.ToString(), out result);

            return result;
        }

        public static int? AsNullableInt(this object obj)
        {
            if (obj is int)
                return (int)obj;

            if (obj != null)
            {
                int result;
                return Int32.TryParse(obj.ToString(), out result)
                    ? new int?(result)
                    : null;
            }

            return null;
        }

        public static Boolean IsAnonymousType(this Type type)
        {
            Boolean hasCompilerGeneratedAttribute = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any();
            Boolean nameContainsAnonymousType = type.FullName.Contains("AnonymousType");
            Boolean isAnonymousType = hasCompilerGeneratedAttribute && nameContainsAnonymousType;

            return isAnonymousType;
        }
    }
}