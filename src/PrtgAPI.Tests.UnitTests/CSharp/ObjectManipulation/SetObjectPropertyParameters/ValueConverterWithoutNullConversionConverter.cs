using PrtgAPI.Request.Serialization.ValueConverters;

namespace PrtgAPI.Tests.UnitTests.ObjectManipulation
{
    class ValueConverterWithoutNullConversionConverter : IValueConverter
    {
        internal static ValueConverterWithoutNullConversionConverter Instance = new ValueConverterWithoutNullConversionConverter();

        public object Serialize(object value)
        {
            if (value == null)
                return null;

            return $"serialized{value}";
        }

        public object Deserialize(object value)
        {
            if (value == null)
                return null;

            return $"deserialized{value}";
        }
    }
}
