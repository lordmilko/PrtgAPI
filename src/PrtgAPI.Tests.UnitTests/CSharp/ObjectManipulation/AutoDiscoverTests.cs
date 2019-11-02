using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.UnitTests.ObjectManipulation
{
    [TestClass]
    public class AutoDiscoverTests : BaseTest
    {
        [UnitTest]
        [TestMethod]
        public void AutoDiscover_CanExecute() =>
            Execute(c => c.AutoDiscover(1001), "api/discovernow.htm?id=1001");

        [UnitTest]
        [TestMethod]
        public async Task AutoDiscover_CanExecuteAsync() =>
            await ExecuteAsync(async c => await c.AutoDiscoverAsync(1001), "api/discovernow.htm?id=1001");
    }
}
