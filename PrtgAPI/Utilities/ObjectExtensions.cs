using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PrtgAPI.Utilities
{
    static class ObjectExtensions
    {
        public static bool IsIEnumerable(this object obj)
        {
            return obj is IEnumerable && !(obj is string);
        }

        public static IEnumerable<object> ToIEnumerable(this object obj)
        {
            return ((IEnumerable) obj).Cast<object>();
        }
    }
}
