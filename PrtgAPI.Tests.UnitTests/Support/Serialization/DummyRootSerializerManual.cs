using System.Xml;

namespace PrtgAPI.Tests.UnitTests.Support.Serialization
{
    class DummyRootSerializerManual : DummyElementSerializerManual<DummyElementRoot>
    {
        object value;

        public DummyRootSerializerManual(XmlReader reader) : base(reader)
        {
        }

        protected override void Init()
        {
            value = reader.NameTable.Add("value");

            base.Init();
        }

        protected override bool ProcessItemElements(DummyElement<DummyElementRoot> obj, bool[] flagArray)
        {
            if (!flagArray[0] && ElementName == property)
            {
                obj.Property = ReadElement<DummyElementRoot>(1, (s, f) => false, ProcessDummyRootElements, ValidateDummyRoot); ;
                flagArray[0] = true;
                return true;
            }

            return false;
        }

        private bool ProcessDummyRootElements(DummyElementRoot obj, bool[] flagArray)
        {
            if (!flagArray[0] && ElementName == value)
            {
                obj.Value = GetValue<bool>(ReadElementString);
                flagArray[0] = true;
                return true;
            }

            return false;
        }

        private void ValidateDummyRoot(bool[] flagArray)
        {
            ValidateSingleDummy<bool>(flagArray);
        }
    }
}
