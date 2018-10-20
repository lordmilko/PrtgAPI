using System.Xml;

namespace PrtgAPI.Tests.UnitTests.Support.Serialization
{
    class DummyAttributeSerializerManual<T> : DummySerializerManualBase<DummyAttribute<T>>
    {
        public DummyAttributeSerializerManual(XmlReader reader) : base(reader)
        {
        }

        protected override DummyAttribute<T> DeserializeItem()
        {
            return ReadElement<DummyAttribute<T>>(1, ProcessItemAttributes, (s, f) => false, ValidateDummy);
        }

        protected virtual bool ProcessItemAttributes(DummyAttribute<T> obj, bool[] flagArray)
        {
            if (!flagArray[0] && AttributeName == property)
            {
                obj.Property = GetValue<T>(ReadAttributeString);
                flagArray[0] = true;
                return true;
            }

            return false;
        }

        protected override void ValidateDummy(bool[] flagArray)
        {
            ValidateSingleDummy<T>(flagArray);
        }
    }
}
