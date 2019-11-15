using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace PrtgAPI.Linq
{
    internal static class EnumerableEx
    {
        internal static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector,
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
        internal static IEnumerable<TSource> ExceptBy<TSource, TKey>(this IEnumerable<TSource> first,
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

        internal static IEnumerable<TSource> ToCached<TSource>(this IEnumerable<TSource> source) =>
            new CachedEnumerableIterator<TSource>(source);

        internal static T SingleObject<T>(this List<T> source, object value, string property = "ID") where T : IObject
        {
            if (source.Count == 1)
                return source.Single();

            var desc = IObjectExtensions.GetTypeDescription(typeof(T));

            if (source.Count == 0)
                throw new InvalidOperationException($"Failed to retrieve {desc.ToLower()} with {property} '{value}': {desc} does not exist.");

            var str = source.Select(s => $"{s} ({s.GetId()})");

            throw new InvalidOperationException($"Failed to retrieve {desc.ToLower()} with {property} '{value}': Multiple {desc.ToLower()}s were returned: {(string.Join(", ", str))}.");
        }

        internal static T[] WithoutNull<T>(this T[] source)
        {
            if (source.Any(v => v == null))
                return source.Where(v => v != null).ToArray();

            return source;
        }

        internal static List<T> WithoutNull<T>(this List<T> source)
        {
            if (source.Any(v => v == null))
                return source.Where(v => v != null).ToList();

            return source;
        }

        internal static ICollection<T> AsCollection<T>(this IEnumerable<T> source)
        {
            if (source is ICollection<T>)
                return (ICollection<T>) source;

            return source.ToList();
        }

        internal static ReadOnlyCollection<T> ToReadOnly<T>(this IEnumerable<T> source)
        {
            var list = source as IList<T>;

            if (list != null)
                return new ReadOnlyCollection<T>(list);

            return new ReadOnlyCollection<T>(source.ToList());
        }
    }
}
