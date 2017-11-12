using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.CSharp
{
    [TestClass]
    public class DeleteTests : BaseTest
    {
        [TestMethod]
        public void Delete_CanExecute()
        {
            var client = Initialize_Client(new BasicResponse(string.Empty));

            client.DeleteObject(1001);
        }

        [TestMethod]
        public async Task Delete_CanExecuteAsync()
        {
            var client = Initialize_Client(new BasicResponse(string.Empty));

            await client.DeleteObjectAsync(1001);
        }
    }
}
