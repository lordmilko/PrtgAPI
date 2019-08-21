using PrtgAPI.Request;

namespace PrtgAPI.Tests.UnitTests.ObjectManipulation
{
    class FakeSerializable : ISerializable
    {
        public string GetSerializedFormat()
        {
            return "serializedValue";
        }
    }
}
