using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectManipulation
{
    [TestClass]
    public class CloneTests : BaseTest
    {
        [UnitTest]
        [TestMethod]
        public void Clone_CanClone()
        {
            var client = Initialize_Client(new CloneResponse());

            client.CloneObject(1, "New Name", 3);
        }

        [UnitTest]
        [TestMethod]
        public async Task Clone_CanCloneAsync()
        {
            var client = Initialize_Client(new CloneResponse());

            await client.CloneObjectAsync(1, "New Name", 3);
        }

        [UnitTest]
        [TestMethod]
        public async Task Clone_CanCloneDeviceAsync()
        {
            var client = Initialize_Client(new CloneResponse());

            await client.CloneObjectAsync(1, "New Name", "host", 3);
        }

        [UnitTest]
        [TestMethod]
        public void Clone_CanGetId_WhenObjectIsInvalid()
        {
            var client = Initialize_Client(new CloneResponse("https://prtg.example.com/error.htm?errormsg=the object is currently not valid&errorurl=/object.htm?id=9999"));

            var id = client.CloneObject(3, "New Name", 5);

            Assert.AreEqual(9999, id);
        }
    }
}
