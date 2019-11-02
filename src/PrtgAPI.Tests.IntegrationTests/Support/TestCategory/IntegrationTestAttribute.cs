using PrtgAPI.Tests.UnitTests.Support;

namespace PrtgAPI.Tests.IntegrationTests
{
    class IntegrationTestAttribute : CustomTestCategoryAttribute
    {
        public IntegrationTestAttribute(params string[] category) : base("IntegrationTest", category)
        {
        }
    }
}
