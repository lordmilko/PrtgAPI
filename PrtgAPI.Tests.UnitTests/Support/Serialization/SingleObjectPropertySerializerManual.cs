using System;

namespace PrtgAPI.Tests.UnitTests.Support.Serialization
{
    class SingleObjectPropertySerializerManual : XmlSerializerManual
    {
        public SingleObjectPropertySerializerManual() : base(null)
        {
        }

        public override object Deserialize(bool validateValueTypes)
        {
            throw new NotImplementedException();
        }

        protected override void Init()
        {
        }
    }
}
