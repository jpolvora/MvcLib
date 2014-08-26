using System.Collections.Generic;
using System.Dynamic;

namespace MvcLib.Common
{
    public class DynamicDictionaryWrapper : DynamicObject
    {
        private readonly IDictionary<string, object>[] _wrapped;

        public IDictionary<string, object>[] GetWrapped
        {
            get { return _wrapped; }
        }

        public DynamicDictionaryWrapper(params IDictionary<string, object>[] wrapped)
        {
            _wrapped = wrapped;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            foreach (var dictionary in _wrapped)
            {
                foreach (var o in dictionary)
                {
                    yield return o.Key;
                }
            }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            foreach (var dictionary in _wrapped)
            {
                if (dictionary.ContainsKey(binder.Name))
                {
                    result = dictionary[binder.Name];
                    return true;
                }
            }
            result = null;
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            var key = indexes[0].ToString();

            foreach (var dictionary in _wrapped)
            {
                if (dictionary.ContainsKey(key))
                {
                    result = dictionary[key];
                    return true;
                }
            }

            result = null;
            return true;
        }
    }
}