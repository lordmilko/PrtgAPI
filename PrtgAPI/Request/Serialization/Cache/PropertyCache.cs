using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PrtgAPI.Objects.Deserialization.Cache
{
    class PropertyCache
    {
        public PropertyInfo Property { get; set; }

        private Dictionary<Type, List<Attribute>> attributes;

        public Dictionary<Type, List<Attribute>> Attributes
        {
            get
            {
                if (attributes == null)
                {
                    attributes = new Dictionary<Type, List<Attribute>>();

                    var attribs = Property.GetCustomAttributes(true).Cast<Attribute>();

                    var groups = attribs.GroupBy(a => a.GetType());

                    foreach (var group in groups)
                    {
                        attributes[group.Key] = group.ToList();
                    }
                }

                return attributes;
            }
        }

        public List<Attribute> GetAttributes(Type type)
        {
            if (Attributes.ContainsKey(type))
                return Attributes[type];

            return new List<Attribute>();
        }

        public PropertyCache(PropertyInfo property)
        {
            Property = property;
        }
    }
}
