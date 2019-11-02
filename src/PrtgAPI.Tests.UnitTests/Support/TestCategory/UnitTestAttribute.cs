using PrtgAPI.Tests.UnitTests.Support;

namespace PrtgAPI.Tests.UnitTests
{
    public class UnitTestAttribute : CustomTestCategoryAttribute
    {
        public UnitTestAttribute(params string[] category) : base("UnitTest", category)
        {
        }
    }
}
