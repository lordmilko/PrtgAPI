using System;
using System.Collections.Generic;
using System.Linq;
using PrtgAPI.Request.Serialization;
using PrtgAPI.Utilities;

namespace PrtgAPI.Reflection.Cache
{
    struct CacheValue<T>
    {
        public Type Underlying { get; }

        public T Cache { get; }

        public CacheValue(Type underlying, T cache)
        {
            Underlying = underlying;
            Cache = cache;
        }
    }

    static class ReflectionCacheManager
    {
        private static readonly Dictionary<Type, CacheValue<TypeCache>> typeCache = new Dictionary<Type, CacheValue<TypeCache>>();
        private static readonly Dictionary<Type, CacheValue<EnumCache>> enumCache = new Dictionary<Type, CacheValue<EnumCache>>();
        private static readonly Dictionary<Type, CacheValue<EnumXmlCache>> enumXmlCache = new Dictionary<Type, CacheValue<EnumXmlCache>>();
        private static readonly Dictionary<Type, CacheValue<EnumNameCache>> enumNameCache = new Dictionary<Type, CacheValue<EnumNameCache>>();
        private static readonly Dictionary<Type, CacheValue<List<XmlMapping>>> mappingCache = new Dictionary<Type, CacheValue<List<XmlMapping>>>();
        private static readonly Dictionary<Type, CacheValue<Type>> arrayPropertyCache = new Dictionary<Type, CacheValue<Type>>();

        private static readonly object lockTypeCache = new object();
        private static readonly object lockEnumCache = new object();
        private static readonly object lockEnumXmlCache = new object();
        private static readonly object lockEnumNameCache = new object();
        private static readonly object lockMappingCache = new object();
        private static readonly object lockArrayPropertyCache = new object();

        public static TypeCache Get(Type type)
        {
            //Enum Caches have additional properties over regular type caches. To avoid having to reconstruct
            //all of the properties known to a regular type cache in the event we need to create an enum cache,
            //we just retrieve the enum cache in the first place
            if (type.IsEnum)
                return GetValue(lockEnumCache, type, enumCache, t => new EnumCache(t)).Cache;

            return GetValue(lockTypeCache, type, typeCache, t => new TypeCache(t)).Cache;
        }

        public static CacheValue<TypeCache> GetTypeCache(Type type)
        {
            return GetValue(lockTypeCache, type, typeCache, t => new TypeCache(t));
        }

        public static CacheValue<EnumCache> GetEnumCache(Type type)
        {
            return GetValue(lockEnumCache, type, enumCache, t => new EnumCache(t));
        }

        public static Type GetArrayPropertyType(Type type)
        {
            if (type == null)
                return null;

            return GetValue(lockArrayPropertyCache, type, arrayPropertyCache, t =>
            {
                if (t.IsArray)
                    return GetArrayPropertyType(t.GetElementType());

                var underlying = Nullable.GetUnderlyingType(t);

                if (underlying != null)
                    return GetArrayPropertyType(underlying);

                var arrayProperties = Get(typeof(DynamicParameterPropertyTypes)).Properties;

                var info = arrayProperties.FirstOrDefault(p => p.Property.PropertyType.GetElementType().Name == t.Name)?.Property;

                if (t.IsEnum || underlying?.IsEnum == true)
                {
                    var t2 = underlying ?? t;

                    return t2.MakeArrayType();
                }

                return info?.PropertyType;
            }).Cache;
        }

        public static CacheValue<EnumXmlCache> GetEnumXml(Type type)
        {
            return GetValue(lockEnumXmlCache, type, enumXmlCache, t => new EnumXmlCache(t));
        }

        public static CacheValue<EnumNameCache> GetEnumName(Type type)
        {
            return GetValue(lockEnumNameCache, type, enumNameCache, t => new EnumNameCache(t));
        }

        public static FieldCache GetEnumFieldCache(Enum value)
        {
            var cache = GetValue(lockEnumCache, value.GetType(), enumCache, t => new EnumCache(t));

            FieldCache fieldCache;

            if (cache.Cache.ValueCache.TryGetValue(value, out fieldCache))
                return fieldCache;

            return null;
        }

        public static CacheValue<List<XmlMapping>> Map(Type type)
        {
            return GetValue(lockMappingCache, type, mappingCache, XmlMapping.GetMappings);
        }

        private static CacheValue<T> GetValue<T>(object lockObj, Type type, Dictionary<Type, CacheValue<T>> dict, Func<Type, T> init)
        {
            lock (lockObj)
            {
                CacheValue<T> value;

                if (!dict.TryGetValue(type, out value))
                {
                    //The type doesn't exist in the dictionary. Could it have been nullable?
                    var underlying = Nullable.GetUnderlyingType(type);

                    T cache;

                    if (underlying != null)
                    {
                        if (dict.TryGetValue(underlying, out value))
                        {
                            //The underlying type existed! Map the nullable type to the underlying type's value
                            var nullableValue = new CacheValue<T>(underlying, value.Cache);
                            dict[type] = nullableValue;
                            return nullableValue;
                        }

                        //The underlying type didn't exist. Map both the nullable and non nullable type to the same value
                        cache = init(underlying);
                        value = new CacheValue<T>(underlying, cache);
                        dict[type] = value;
                        dict[underlying] = new CacheValue<T>(null, cache);

                        return value;
                    }

                    //It's a non nullable type that doesn't exist; let's initialize it!
                    cache = init(type);
                    value = new CacheValue<T>(null, cache);
                    dict[type] = value;
                    return value;
                }

                return value;
            }
        }
    }
}
