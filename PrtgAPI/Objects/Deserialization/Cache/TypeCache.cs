using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PrtgAPI.Objects.Deserialization.Cache
{
    class TypeCache
    {
        public Type Type { get; set; }

        private List<PropertyCache> properties;

        public List<PropertyCache> Properties
        {
            get
            {
                if (properties == null)
                {
                    var props = Type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                    properties = props.Select(p => new PropertyCache(p)).ToList();
                }

                return properties;
            }
        }

        private List<FieldCache> fields;

        public List<FieldCache> Fields
        {
            get
            {
                if (fields == null)
                {
                    fields = Type.GetFields().Select(f => new FieldCache(f)).ToList();
                }

                return fields;
            }
        }

        public TypeCache(Type type)
        {
            Type = type;
        }
    }
}
