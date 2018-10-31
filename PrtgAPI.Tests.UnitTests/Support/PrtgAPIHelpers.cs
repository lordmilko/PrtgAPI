using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using PrtgAPI.Attributes;
using PrtgAPI.Linq;
using PrtgAPI.Linq.Expressions.Visitors;
using PrtgAPI.Parameters;
using PrtgAPI.Reflection;
using PrtgAPI.Reflection.Cache;
using PrtgAPI.Utilities;

namespace PrtgAPI.Tests.UnitTests.Support
{
    public static class PrtgAPIHelpers
    {
        public static IEqualityComparer<Log> LogEqualityComparer() => new LogEqualityComparer();

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey> comparer = null)
        {
            return EnumerableEx.DistinctBy(source, keySelector, comparer);
        }

        public static IEnumerable<TSource> ExceptBy<TSource, TKey>(this IEnumerable<TSource> first,
            IEnumerable<TSource> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer = null)
        {
            return EnumerableEx.ExceptBy(first, second, keySelector, comparer);
        }

        public static IEnumerable<PropertyInfo> GetNormalProperties(this Type type) => ReflectionExtensions.GetNormalProperties(type).Where(p => p.Property.GetGetMethod() != null).Select(p => p.Property);

        public static object GetInternalField(this object obj, string name) => ReflectionExtensions.GetInternalField(obj, name);

        public static object GetInternalProperty(this object obj, string name) => ReflectionExtensions.GetInternalProperty(obj, name);

        public static PropertyInfo GetInternalPropertyInfo(this object obj, string name) => ReflectionExtensions.GetInternalPropertyInfo(obj, name);

        public static Expression ToClientExpression(this Expression expr)
        {
            return ClientTreeBuilder.Parse(expr);
        }

        public static Func<T, object> ToMemberAccessLambda<T>(Property property)
        {
            var typeProps = ReflectionCacheManager.Get(typeof(T)).Properties;
            var propertyCache = typeProps.First(
                p => p.GetAttributes<PropertyParameterAttribute>().Any(a => a.Property.Equals(property))
            );

            var obj = Expression.Parameter(typeof(T));
            var memberAccess = Expression.MakeMemberAccess(obj, propertyCache.Property);

            var lambda = Expression.Lambda<Func<T, object>>(Expression.Convert(memberAccess, typeof(object)), obj);

            return lambda.Compile();
        }

        public static void FoldObject(PrtgClient client, int objectId, bool fold) => client.FoldObject(objectId, fold);

        public static IParameters GetPassHashParameters(string password) => new PassHashParameters(password);

        public static object GetTriggerChannelSource(TriggerChannel channel) => channel.channel;

        public static IEnumerable<TObject> TakeIterator<TObject, TParam>(int takeCount,
            TParam parameters,
            Func<TParam, Func<int>, IEnumerable<TObject>> streamer,
            Func<int> getCount,
            Func<IEnumerable<TObject>, IEnumerable<TObject>> postProcessor,
            bool stream = true) where TParam : PageableParameters
        {
            return new TakeIterator<TObject, TParam>(takeCount, parameters, streamer, getCount, postProcessor, null, stream);
        }

        public static IEnumerable<Sensor> StreamObjects(PrtgClient client, SensorParameters parameters, bool serial, Func<int> getCount)
        {
            return client.ObjectEngine.StreamObjects<Sensor, SensorParameters>(parameters, serial, getCount);
        }

        public static T ToEnum<T>(this string value)
        {
            return EnumExtensions.ToEnum<T>(value);
        }

        public static Enum GetPropertyCategory(ObjectProperty property)
        {
            return property.GetEnumAttribute<CategoryAttribute>().Name;
        }
    }
}
