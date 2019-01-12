using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.UnitTests.ObjectManipulation
{
    [TestClass]
    public class RefreshTests : BaseTest
    {
        [TestMethod]
        [TestCategory("UnitTest")]
        public void Refresh_CanExecute() =>
            Execute(c => c.RefreshObject(1001), "api/scannow.htm?id=1001");

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Refresh_CanExecuteAsync() =>
            await ExecuteAsync(async c => await c.RefreshObjectAsync(1001), "api/scannow.htm?id=1001");
    }
}
