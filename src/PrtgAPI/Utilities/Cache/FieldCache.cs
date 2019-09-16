using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace PrtgAPI.Reflection.Cache
{
    [ExcludeFromCodeCoverage]
    [DebuggerDisplay("{Field}")]
    class FieldCache : AttributeCache
    {
        public FieldInfo Field { get; set; }

        private Lazy<Func<object, object>> getValue;
        private Lazy<Action<object, object>> setValue;

        public FieldCache(FieldInfo field)
        {
            Field = field;

            getValue = new Lazy<Func<object, object>>(() => ReflectionExtensions.CreateGetValue(Field));
            setValue = new Lazy<Action<object, object>>(() => ReflectionExtensions.CreateSetValue(Field, Field.FieldType));
        }

        public void SetValue(object obj, object value)
        {
            setValue.Value(obj, value);
        }

        public object GetValue(object obj)
        {
            return getValue.Value(obj);
        }

        protected override MemberInfo attributeSource => Field;
    }
}
