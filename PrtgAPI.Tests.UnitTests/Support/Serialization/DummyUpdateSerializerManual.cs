using System;
using System.Xml;

namespace PrtgAPI.Tests.UnitTests.Support.Serialization
{
    class DummyUpdateSerializerManual<T1, T2> : DummySerializerManualBase<DummyUpdate<T1, T2>>
    {
        object objid;

        DummyUpdate<T1, T2> obj;

        public DummyUpdateSerializerManual(XmlReader reader) : base(reader)
        {
        }

        protected override void Init()
        {
            objid = reader.NameTable.Add("objid");

            base.Init();
        }

        public void Update(DummyUpdate<T1, T2> target)
        {
            obj = target;
            reader.ReadToFollowing("item");
            ReadElement<DummyUpdate<T1, T2>>(2, (s, f) => false, ProcessItemElements, ValidateDummy);
        }

        protected override T CreateElement<T>()
        {
            return (T)(object)obj;
        }

        protected override DummyUpdate<T1, T2> DeserializeItem()
        {
            return ReadElement<DummyUpdate<T1, T2>>(2, (s, f) => false, ProcessItemElements, ValidateDummy);
        }

        protected virtual bool ProcessItemElements(DummyUpdate<T1, T2> obj, bool[] flagArray)
        {
            if (!flagArray[1] && ElementName == property)
            {
                obj.Property2 = GetValue<T2>(ReadElementString);
                flagArray[1] = true;
                return true;
            }

            return false;
        }

        protected override void ValidateDummy(bool[] flagArray)
        {
            if (!flagArray[1])
            {
                if (typeof(T2).IsValueType && Nullable.GetUnderlyingType(typeof(T2)) == null)
                {
                    ElementName = property;
                    throw Fail(null, null, typeof(T2));
                }
            }
        }
    }
}
