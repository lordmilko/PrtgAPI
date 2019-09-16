using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace PrtgAPI.Reflection.Cache
{
    class PropertyCache : AttributeCache
    {
        public PropertyInfo Property { get; set; }

        public Type Type => Property.PropertyType;

        private Lazy<Func<object, object>> getValue;
        private Lazy<Action<object, object>> setValue;

        public PropertyCache(PropertyInfo property)
        {
            Property = property;

            getValue = new Lazy<Func<object, object>>(() => ReflectionExtensions.CreateGetValue(Property));
            setValue = new Lazy<Action<object, object>>(() => ReflectionExtensions.CreateSetValue(Property, Property.PropertyType));
        }

        public void SetValue(object obj, object value)
        {
            setValue.Value(obj, value);
        }

        public object GetValue(object obj)
        {
            return getValue.Value(obj);
        }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return Property.ToString();
        }

        protected override MemberInfo attributeSource => Property;
    }
}
