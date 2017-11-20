using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    [TestClass]
    public class RenameTests : BaseTest
    {
        [TestMethod]
        public void Rename_CanExecute()
        {
            var client = Initialize_Client(new BasicResponse(string.Empty));

            client.RenameObject(1001, "new name");
        }

        [TestMethod]
        public async Task Rename_CanExecuteAsync()
        {
            var client = Initialize_Client(new BasicResponse(string.Empty));

            await client.RenameObjectAsync(1001, "new name");
        }
    }
}
