using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace PrtgAPI.Reflection.Cache
{
    [ExcludeFromCodeCoverage]
    class FieldCache : AttributeCache
    {
        public FieldInfo Field { get; set; }

        public FieldCache(FieldInfo field)
        {
            Field = field;
        }

        protected override MemberInfo attributeSource => Field;
    }
}
