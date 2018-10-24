using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Reflection.Cache
{
    [ExcludeFromCodeCoverage]
    class EnumNameCache : TypeCache
    {
        private Lazy<Dictionary<string, Enum>> nameCache;

        public Dictionary<string, Enum> NameCache => nameCache.Value;

        public EnumNameCache(Type type) : base(type)
        {
            nameCache = new Lazy<Dictionary<string, Enum>>(GetNames);
        }

        private Dictionary<string, Enum> GetNames()
        {
            var dict = new Dictionary<string, Enum>();

            foreach (var cache in Fields)
            {
                if (cache.Field.FieldType.IsEnum)
                {
                    var value = (Enum)cache.Field.GetValue(null);

                    dict[value.ToString()] = value;
                }
            }

            return dict;
        }
    }
}
