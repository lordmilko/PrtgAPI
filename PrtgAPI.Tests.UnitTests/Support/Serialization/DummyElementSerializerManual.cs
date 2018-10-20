using System.Xml;

namespace PrtgAPI.Tests.UnitTests.Support.Serialization
{
    class DummyElementSerializerManual<T> : DummySerializerManualBase<DummyElement<T>>
    {
        public DummyElementSerializerManual(XmlReader reader) : base(reader)
        {
        }

        protected override DummyElement<T> DeserializeItem()
        {
            return ReadElement<DummyElement<T>>(1, (s, f) => false, ProcessItemElements, ValidateDummy);
        }

        protected virtual bool ProcessItemElements(DummyElement<T> obj, bool[] flagArray)
        {
            if (!flagArray[0] && ElementName == property)
            {
                obj.Property = GetValue<T>(ReadElementString);
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
