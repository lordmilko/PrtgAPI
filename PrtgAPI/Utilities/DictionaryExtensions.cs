using System;
using System.Collections.Generic;

namespace PrtgAPI.Utilities
{
    internal static class DictionaryExtensions
    {
        internal static bool TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, out TValue value, bool ignoreCase, bool trimName = false)
        {
            if (ignoreCase)
            {
                var keyStr = key.ToString();

                if (trimName)
                    keyStr = keyStr.TrimEnd('_');

                foreach (var kv in dictionary)
                {
                    var kStr = trimName ? kv.Key.ToString().TrimEnd('_') : kv.Key.ToString();

                    if (string.Equals(kStr, keyStr, StringComparison.InvariantCultureIgnoreCase))
                    {
                        value = kv.Value;
                        return true;
                    }
                }

                value = default(TValue);

                return false;
            }
            else
                return dictionary.TryGetValue(key, out value);
        }
    }
}
