using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace PrtgAPI.Objects.Deserialization.Cache
{
    [ExcludeFromCodeCoverage]
    class FieldCache
    {
        public FieldInfo Field { get; set; }

        private Dictionary<Type, List<Attribute>> attributes;

        public Dictionary<Type, List<Attribute>> Attributes
        {
            get
            {
                if (attributes == null)
                {
                    attributes = new Dictionary<Type, List<Attribute>>();

                    var attribs = Field.GetCustomAttributes(true).Cast<Attribute>();

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
        public FieldCache(FieldInfo field)
        {
            Field = field;
        }
    }
}
