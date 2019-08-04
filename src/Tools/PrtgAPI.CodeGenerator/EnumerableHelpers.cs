using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PrtgAPI.CodeGenerator
{
    static class EnumerableHelpers
    {
        public static ReadOnlyCollection<T> ToReadOnlyList<T>(this IEnumerable<T> list)
        {
            return ToReadOnlyList(list.ToList());
        }

        public static ReadOnlyCollection<T> ToReadOnlyList<T>(this IList<T> list)
        {
            return new ReadOnlyCollection<T>(list);
        }
    }
}
