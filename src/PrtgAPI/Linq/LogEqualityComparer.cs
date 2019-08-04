using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using PrtgAPI.Reflection;
using PrtgAPI.Reflection.Cache;
using PrtgAPI.Utilities;

namespace PrtgAPI.Linq
{
    class LogEqualityComparer : EqualityComparer<Log>
    {
        static List<PropertyCache> caches = typeof(Log).GetTypeCache().Properties.Where(p => p.Property.GetSetMethod() != null).ToList();

        private Func<Log, Log, bool> areEqual;

        public LogEqualityComparer()
        {
            areEqual = CreateComparer();
        }

        private Func<Log, Log, bool> CreateComparer()
        {
            var properties = typeof(Log).GetTypeCache().Properties.Where(p => p.Property.GetSetMethod() != null).Select(p => p.Property).ToList();

            var log1 = Expression.Parameter(typeof(Log), "log1");
            var log2 = Expression.Parameter(typeof(Log), "log2");

            Expression final = null;

            foreach (var property in properties)
            {
                var log1Access = Expression.MakeMemberAccess(log1, property);
                var log2Access = Expression.MakeMemberAccess(log2, property);

                Expression eq;

                if ((typeof(IStructuralEquatable).IsAssignableFrom(property.PropertyType)))
                {
                    var comparer = StructuralComparisons.StructuralEqualityComparer;
                    var equals = comparer.GetType().GetMethod("Equals", new[] { typeof(object), typeof(object) });

                    //var equals = typeof(StructuralComparisons).GetMethod("Equals", BindingFlags.Static | BindingFlags.Public);

                    eq = Expression.Call(Expression.Constant(comparer), equals, log1Access, log2Access);
                }
                else
                    eq = Expression.Equal(log1Access, log2Access);

                if (final == null)
                    final = eq;
                else
                    final = Expression.AndAlso(final, eq);
            }

            var lambda = Expression.Lambda<Func<Log, Log, bool>>(
                final,
                log1,
                log2
            );

            return lambda.Compile();
        }

        public override bool Equals(Log x, Log y)
        {
            return areEqual(x, y);
        }

        public override int GetHashCode(Log obj)
        {
            var array = new List<Tuple<string, int>>();

            unchecked
            {
                int result = 17;

                foreach (var cache in caches)
                {
                    var value = cache.GetValue(obj) ?? 0;

                    if (value.IsIEnumerable())
                    {
                        foreach (var v in (IEnumerable)value)
                        {
                            result = result * 31 + v.GetHashCode();
                        }
                    }
                    else
                        result = result * 31 + value.GetHashCode();

                    array.Add(Tuple.Create(cache.Property.Name, result));
                }

                return result;
            }
        }

        internal static string Stringify(Log log)
        {
            return string.Join("_", caches.Select(c => c.GetValue(log)));
        }
    }
}