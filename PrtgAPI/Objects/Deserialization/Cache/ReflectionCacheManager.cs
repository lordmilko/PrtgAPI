using System;
using System.Collections.Generic;

namespace PrtgAPI.Objects.Deserialization.Cache
{
    static class ReflectionCacheManager
    {
        private static Dictionary<Type, TypeCache> typeCache = new Dictionary<Type, TypeCache>();
        private static Dictionary<Type, EnumXmlCache> enumCache = new Dictionary<Type, EnumXmlCache>();
        private static Dictionary<Type, List<XmlMapping>> mappingCache = new Dictionary<Type, List<XmlMapping>>();

        public static TypeCache Get(Type type)
        {
            return GetValue(type, typeCache, t => new TypeCache(t));
        }

        public static EnumXmlCache GetEnumXml(Type type)
        {
            return GetValue(type, enumCache, t => new EnumXmlCache(t));
        }

        public static List<XmlMapping> Map(Type type)
        {
            return GetValue(type, mappingCache, XmlMapping.GetMappings);
        }

        private static T GetValue<T>(Type type, Dictionary<Type, T> dict, Func<Type, T> init)
        {
            T value;

            if (!dict.TryGetValue(type, out value))
            {
                value = init(type);
                dict[type] = value;
                return value;
            }

            return dict[type];
        }
    }
}
