using System;
using System.Collections.Generic;
using System.Linq;

namespace PrtgAPI.Utilities
{
    static class DictionaryExtensions
    {
        public static bool TryGetValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, out TValue value, bool ignoreCase, bool trimName = false)
        {
            if (ignoreCase)
            {
                var keyStr = key.ToString();

                if (trimName)
                    keyStr = keyStr.TrimEnd('_');

                var val = dictionary.Where(
                    k =>
                    {
                        var kStr = trimName ? k.Key.ToString().TrimEnd('_') : k.Key.ToString();

                        return string.Equals(kStr, keyStr,
                            StringComparison.InvariantCultureIgnoreCase);
                    }).Select(v => v.Value).FirstOrDefault();

                if (val != null)
                {
                    value = val;
                    return true;
                }

                value = default(TValue);

                return false;
            }
            else
                return dictionary.TryGetValue(key, out value);
        }
    }
}
