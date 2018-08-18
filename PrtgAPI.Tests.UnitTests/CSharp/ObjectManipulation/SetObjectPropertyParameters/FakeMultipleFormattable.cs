namespace PrtgAPI.Tests.UnitTests.ObjectTests
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