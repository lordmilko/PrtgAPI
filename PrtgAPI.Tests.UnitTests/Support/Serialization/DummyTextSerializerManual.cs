using System;
using System.Xml;

namespace PrtgAPI.Tests.UnitTests.Support.Serialization
{
    class DummyTextSerializerManual<T> : DummySerializerManualBase<DummyText<T>>
    {
        public DummyTextSerializerManual(XmlReader reader) : base(reader)
        {
        }

        bool textFound = false;

        protected override void Init()
        {
            base.Init();
            property = reader.NameTable.Add("item");
        }

        protected override DummyText<T> DeserializeItem()
        {
            return ReadElement<DummyText<T>>(1, (s, f) => false, (s, f) => false, ValidateDummy);
        }

        protected override bool ProcessText<TObj>(TObj obj)
        {
            ((DummyText<T>)(object)obj).Value = GetValue<T>(ReadTextString);
            textFound = true;
            return true;
        }

        protected override void ValidateDummy(bool[] flagArray)
        {
            if (textFound == false)
            {
                if (typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) == null)
                {
                    ElementName = property;
                    throw Fail(null, null, typeof(T));
                }
            }
        }
    }
}
