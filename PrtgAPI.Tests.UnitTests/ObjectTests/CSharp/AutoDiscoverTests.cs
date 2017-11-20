using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    [TestClass]
    public class AutoDiscoverTests : BaseTest
    {
        [TestMethod]
        public void AutoDiscover_CanExecute()
        {
            var client = Initialize_Client(new BasicResponse(string.Empty));

            client.AutoDiscover(1001);
        }

        [TestMethod]
        public async Task AutoDiscover_CanExecuteAsync()
        {
            var client = Initialize_Client(new BasicResponse(string.Empty));

            await client.AutoDiscoverAsync(1001);
        }
    }
}
