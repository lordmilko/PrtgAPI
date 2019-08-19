using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PrtgAPI.Reflection.Cache
{
    abstract class AttributeCache
    {
        protected abstract MemberInfo attributeSource { get; }

        private Lazy<Dictionary<Type, Attribute[]>> attributes;

        public AttributeCache()
        {
            attributes = new Lazy<Dictionary<Type, Attribute[]>>(GetAttributes);
        }

        private Dictionary<Type, Attribute[]> GetAttributes()
        {
            var dict = new Dictionary<Type, Attribute[]>();

            var attribs = attributeSource.GetCustomAttributes(true).Cast<Attribute>();

            var groups = attribs.GroupBy(a => a.GetType());

            foreach (var group in groups)
            {
                dict[group.Key] = group.ToArray();
            }

            return dict;
        }

        public Dictionary<Type, Attribute[]> Attributes => attributes.Value;

        public TAttribute[] GetAttributes<TAttribute>() where TAttribute : Attribute
        {
            Attribute[] value;

            if (Attributes.TryGetValue(typeof(TAttribute), out value))
                return Array.ConvertAll(value, a => (TAttribute)a);

            return new TAttribute[0];
        }

        public TAttribute GetAttribute<TAttribute>(bool allowBase = false) where TAttribute : Attribute
        {
            Attribute[] value;

            if (Attributes.TryGetValue(typeof(TAttribute), out value))
                return (TAttribute) value[0];

            if (allowBase)
                return Attributes.Values.SelectMany(v => v).OfType<TAttribute>().FirstOrDefault();

            return null;
        }
    }
}