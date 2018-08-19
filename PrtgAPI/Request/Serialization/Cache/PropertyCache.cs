using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace PrtgAPI.Request.Serialization.Cache
{
    class PropertyCache : AttributeCache
    {
        public PropertyInfo Property { get; set; }

        private Lazy<Action<object, object>> setValue;

        public PropertyCache(PropertyInfo property)
        {
            Property = property;

            setValue = new Lazy<Action<object, object>>(CreateSetValue);
        }

        private Action<object, object> CreateSetValue()
        {
            var @this = Expression.Parameter(typeof(object), "obj");
            var val = Expression.Parameter(typeof(object), "val");

            var thisCast = Expression.Convert(@this, Property.DeclaringType);
            var valCast = Expression.Convert(val, Property.PropertyType);

            var access = Expression.MakeMemberAccess(thisCast, Property);
            var assignment = Expression.Assign(access, valCast);

            var lambda = Expression.Lambda<Action<object, object>>(
                assignment,
                @this,
                val
            );

            return lambda.Compile();
        }

        public void SetValue(object obj, object value)
        {
            setValue.Value(obj, value);
        }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return Property.ToString();
        }

        protected override MemberInfo attributeSource => Property;
    }
}
