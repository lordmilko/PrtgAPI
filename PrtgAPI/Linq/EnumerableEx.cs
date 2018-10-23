using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace PrtgAPI.Linq
{
    static class EnumerableEx
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey> comparer = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            var set = new HashSet<TKey>(comparer);

            foreach (var item in source)
            {
                if (set.Add(keySelector(item)))
                    yield return item;
            }
        }

        [ExcludeFromCodeCoverage]
        public static IEnumerable<TSource> ExceptBy<TSource, TKey>(this IEnumerable<TSource> first,
            IEnumerable<TSource> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer = null)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));

            if (second == null)
                throw new ArgumentNullException(nameof(second));

            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            var set = new HashSet<TKey>(second.Select(keySelector), comparer);

            foreach (var item in first)
            {
                var key = keySelector(item);

                if (set.Contains(key))
                    continue;

                yield return item;

                set.Add(key);
            }
        }

        public static T SingleObject<T>(this List<T> source, object value, string property = "ID") where T : IObject
        {
            if (source.Count == 1)
                return source.Single();

            var desc = IObjectExtensions.GetTypeDescription(typeof(T));

            if(source.Count == 0)
                throw new InvalidOperationException($"Failed to retrieve {desc.ToLower()} with {property} '{value}': {desc} does not exist");

            var str = source.Select(s => $"{s} ({s.GetId()})");

            throw new InvalidOperationException($"Failed to retrieve {desc.ToLower()} with {property} '{value}': Multiple {desc.ToLower()}s were returned: " + string.Join(", ", str));
        }
    }
}
