using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    [TestClass]
    public class AutoDiscoverTests : BaseTest
    {
        [TestMethod]
        public void AutoDiscover_CanExecute() =>
            Execute(c => c.AutoDiscover(1001), "api/discovernow.htm?id=1001");

        [TestMethod]
        public async Task AutoDiscover_CanExecuteAsync() =>
            await ExecuteAsync(async c => await c.AutoDiscoverAsync(1001), "api/discovernow.htm?id=1001");
    }
}
