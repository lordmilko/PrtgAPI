using PrtgAPI.Request.Serialization.ValueConverters;

namespace PrtgAPI.Tests.UnitTests.ObjectManipulation
{
    class ValueConverterWithNullConversionConverter : IValueConverter
    {
        internal static ValueConverterWithNullConversionConverter Instance = new ValueConverterWithNullConversionConverter();

        public object Serialize(object value)
        {
            if (value == null)
                return "serializednull";

            return $"serialized{value}";
        }

        public object Deserialize(object value)
        {
            if (value == null)
                return "deserializednull";

            return $"deserialized{value}";
        }
    }
}
