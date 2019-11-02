using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unit = PrtgAPI.Tests.UnitTests.Infrastructure;

namespace PrtgAPI.Tests.IntegrationTests.Infrastructure
{
    [TestClass]
    public class AssemblyTests : BasePrtgClientTest
    {
        [TestMethod]
        [IntegrationTest]
        public void AllIntegrationTestMethods_HaveIntegrationTestAttribute() => Unit.AssemblyTests.ValidateTestMethodCategory<IntegrationTestAttribute>(GetType().Assembly);
    }
}
