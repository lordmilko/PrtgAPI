using PrtgAPI.Request;

namespace PrtgAPI.Tests.UnitTests.ObjectManipulation
{
    class FakeMultipleFormattable : IFormattableMultiple
    {
        public string GetSerializedFormat()
        {
            return "firstValue";
        }

        public string[] GetSerializedFormats()
        {
            return new[] {"firstValue", "secondValue"};
        }
    }
}