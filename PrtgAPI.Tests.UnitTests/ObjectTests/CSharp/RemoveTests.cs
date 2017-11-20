using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    [TestClass]
    public class RemoveTests : BaseTest
    {
        [TestMethod]
        public void Remove_CanExecute()
        {
            var client = Initialize_Client(new BasicResponse(string.Empty));

            client.RemoveObject(1001);
        }

        [TestMethod]
        public async Task Remove_CanExecuteAsync()
        {
            var client = Initialize_Client(new BasicResponse(string.Empty));

            await client.RemoveObjectAsync(1001);
        }
    }
}
