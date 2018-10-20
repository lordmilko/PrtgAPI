using System;
using System.Xml;
using PrtgAPI.Request.Serialization.ValueConverters;

namespace PrtgAPI.Tests.UnitTests.Support.Serialization
{
    class DummyValueConverterSerializerManual<T> : DummySerializerManualBase<DummyElementValueConverter<T>>
    {
        public DummyValueConverterSerializerManual(XmlReader reader) : base(reader)
        {
        }

        protected override DummyElementValueConverter<T> DeserializeItem()
        {
            return ReadElement<DummyElementValueConverter<T>>(1, (s, f) => false, ProcessItemElements, ValidateDummy);
        }

        protected bool ProcessItemElements(DummyElementValueConverter<T> obj, bool[] flagArray)
        {
            if (!flagArray[0] && ElementName == property)
            {
                obj.Property = GetValue<T>(ReadElementString);
                flagArray[0] = true;
                return true;
            }

            return false;
        }

        protected override TValue GetValue<TValue>(Func<string> getString)
        {
            if (typeof(T) == typeof(double))
                return (TValue)(object)UpDownTimeConverter.Instance.Deserialize(ToDouble(getString()));
            if (typeof(T) == typeof(double?))
                return (TValue)(object)ToNullableDouble_WithUpDownTimeConverter(getString);

            throw new NotImplementedException($"Don't know how to deserialize type '{typeof(TValue).Name}'");
        }

        protected override void ValidateDummy(bool[] flagArray)
        {
            ValidateSingleDummy<T>(flagArray);
        }

        double? ToNullableDouble_WithUpDownTimeConverter(Func<string> getString)
        {
            var str = getString();

            return string.IsNullOrEmpty(str) ? null : (double?)UpDownTimeConverter.Instance.Deserialize(ToDouble(str));
        }
    }
}
