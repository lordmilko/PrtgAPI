using System;
using PrtgAPI.Request.Serialization.ValueConverters;

namespace PrtgAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    class ValueConverterAttribute : Attribute
    {
        private Type type;

        private IValueConverter converter;

        public IValueConverter Converter => converter ?? (converter = (IValueConverter) Activator.CreateInstance(type));

        public ValueConverterAttribute(Type type)
        {
            this.type = type;
        }
    }
}
