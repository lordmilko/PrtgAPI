using System.Xml;
using PrtgAPI.Request.Serialization.ValueConverters;

namespace PrtgAPI.Tests.UnitTests.Support.Serialization
{
    class DummyValueConvertableAndUnConvertableSerializerManual : DummySerializerManualBase<DummyElementValueConvertableAndUnConvertable>
    {
        object propertyConvertable;

        public DummyValueConvertableAndUnConvertableSerializerManual(XmlReader reader) : base(reader)
        {
        }

        protected override void Init()
        {
            propertyConvertable = reader.NameTable.Add("propertyConvertable");

            base.Init();
        }

        protected override DummyElementValueConvertableAndUnConvertable DeserializeItem()
        {
            return ReadElement<DummyElementValueConvertableAndUnConvertable>(2, (s, f) => false, ProcessItemElements, ValidateDummy);
        }

        protected bool ProcessItemElements(DummyElementValueConvertableAndUnConvertable obj, bool[] flagArray)
        {
            if (!flagArray[0] && ElementName == property)
            {
                obj.Property = ToNullableDouble(ReadElementString());
                flagArray[0] = true;
                return true;
            }
            if (!flagArray[1] && ElementName == propertyConvertable)
            {
                obj.PropertyConvertable = ToNullableDouble_WithUpDownTimeConverter();
                flagArray[1] = true;
                return true;
            }

            return false;
        }

        protected override void ValidateDummy(bool[] flagArray)
        {
        }

        double? ToNullableDouble_WithUpDownTimeConverter()
        {
            var str = ReadElementString();

            return string.IsNullOrEmpty(str) ? null : (double?)UpDownTimeConverter.Instance.Deserialize(ToDouble(str));
        }
    }
}
