using System;
using System.ComponentModel;

namespace MvcFromDb.Infra.Misc
{
    public static class TypeUtils
    {
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
    }
}