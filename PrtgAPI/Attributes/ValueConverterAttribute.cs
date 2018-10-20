using System;
using PrtgAPI.Linq.Expressions.Serialization;
using PrtgAPI.Request.Serialization.ValueConverters;

namespace PrtgAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    class ValueConverterAttribute : Attribute
    {
        public Type Type { get; }

        private IValueConverter converter;

        public IValueConverter Converter => converter ?? (converter = (IValueConverter) XmlSerializerMembers.GetValueConverterInstance(Type).GetValue(null));

        public ValueConverterAttribute(Type type)
        {
            Type = type;
        }
    }
}
