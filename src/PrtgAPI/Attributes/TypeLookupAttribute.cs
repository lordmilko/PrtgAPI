using System;

namespace PrtgAPI.Attributes
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    sealed class TypeLookupAttribute : Attribute
    {
        public TypeLookupAttribute(Type @class)
        {
            Class = @class;
        }

        public Type Class { get; private set; }
    }
}
