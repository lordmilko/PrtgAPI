using System;

namespace PrtgAPI.Attributes
{
    enum SecondaryPropertyStrategy
    {
        MultipleSerializable,
        SameValue
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    sealed class SecondaryPropertyAttribute : Attribute
    {
        public Enum Property { get; private set; }

        public SecondaryPropertyStrategy Strategy { get; set; }

        public SecondaryPropertyAttribute(ObjectPropertyInternal property, SecondaryPropertyStrategy strategy)
        {
            Property = property;
            Strategy = strategy;
        }

        public SecondaryPropertyAttribute(string property, Type type, SecondaryPropertyStrategy strategy)
        {
            Property = (Enum)Enum.Parse(type, property);
            Strategy = strategy;
        }
    }
}
