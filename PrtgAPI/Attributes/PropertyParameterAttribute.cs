using System;

namespace PrtgAPI.Attributes
{
    /// <summary>
    /// Specifies a property for use as the name or value of a URL parameter/query string.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    sealed class PropertyParameterAttribute : Attribute
    {
        public Enum Property { get; }

        public PropertyParameterAttribute(Property property)
        {
            Property = property;
        }

        public PropertyParameterAttribute(Parameter parameter)
        {
            Property = parameter;
        }

        public PropertyParameterAttribute(ObjectProperty property)
        {
            Property = property;
        }

        public PropertyParameterAttribute(ChannelProperty property)
        {
            Property = property;
        }

        public PropertyParameterAttribute(ChannelPropertyInternal property)
        {
            Property = property;
        }

        public PropertyParameterAttribute(TriggerProperty property)
        {
            Property = property;
        }

        public PropertyParameterAttribute(string property, Type type)
        {
            Property = (Enum)Enum.Parse(type, property);
        }
    }
}
