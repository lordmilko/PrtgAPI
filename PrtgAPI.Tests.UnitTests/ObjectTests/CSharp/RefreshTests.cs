using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.CSharp
{
    [TestClass]
    public class RefreshTests : BaseTest
    {
        [TestMethod]
        public void Refresh_CanExecute()
        {
            var client = Initialize_Client(new BasicResponse(string.Empty));

            client.RefreshObject(1001);
        }

        [TestMethod]
        public async Task Refresh_CanExecuteAsync()
        {
            var client = Initialize_Client(new BasicResponse(string.Empty));

            await client.RefreshObjectAsync(1001);
        }
    }
}
