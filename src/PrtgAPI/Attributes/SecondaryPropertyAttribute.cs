using System;

namespace PrtgAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    sealed class SecondaryPropertyAttribute : Attribute
    {
        public Enum Property { get; private set; }

        public SecondaryPropertyAttribute(ObjectPropertyInternal property)
        {
            Property = property;
        }

        public SecondaryPropertyAttribute(string property, Type type)
        {
            Property = (Enum)Enum.Parse(type, property);
        }
    }
}
