using System;
using System.Collections.Generic;

namespace PrtgAPI.Reflection.Cache
{
    class EnumCache : TypeCache
    {
        private Lazy<Dictionary<Enum, FieldCache>> valueCache;

        public Dictionary<Enum, FieldCache> ValueCache => valueCache.Value;

        private Dictionary<Enum, FieldCache> GetValues()
        {
            var dict = new Dictionary<Enum, FieldCache>();

            foreach (var cache in Fields)
            {
                if (cache.Field.FieldType.IsEnum)
                {
                    var value = (Enum)cache.Field.GetValue(null);

                    dict[value] = cache;
                }
            }

            return dict;
        }

        public EnumCache(Type type) : base(type)
        {
            valueCache = new Lazy<Dictionary<Enum, FieldCache>>(GetValues);
        }
    }
}