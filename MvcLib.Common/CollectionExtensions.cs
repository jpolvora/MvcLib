using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcLib.Common
{
    public static class CollectionExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
            {
                action(item);
            }
        }
        public static void ForEach<T, TT>(this Dictionary<T, TT> dictionary, Action<KeyValuePair<T, TT>> action)
        {
            foreach (var t in dictionary)
            {
                action(t);    
            }
        }
    }
}
