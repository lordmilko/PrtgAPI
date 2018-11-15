using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectData
{
    [TestClass]
    public class SensorTypeTests : BaseTest
    {
        [TestMethod]
        public void SensorType_CanExecute()
        {
            var client = Initialize_Client(new AddressValidatorResponse("api/sensortypes.json?id=3002"));

            var types = client.GetSensorTypes(3002);

            Assert.AreEqual(3, types.Count, "Did not return expected number of types");

            Assert.AreEqual("ptfadsreplfailurexml", types[0].Id, "Id was not correct");
            Assert.AreEqual("Active Directory Replication Errors", types[0].Name, "Name was not correct");
            Assert.AreEqual("Checks Windows domain controllers for replication errors", types[0].Description, "Description was not correct");
        }

        [TestMethod]
        public async Task SensorType_CanExecuteAsync()
        {
            var client = Initialize_Client(new AddressValidatorResponse("api/sensortypes.json?id=3002"));

            var types = await client.GetSensorTypesAsync(3002);

            Assert.AreEqual(3, types.Count, "Did not return expected number of types");

            Assert.AreEqual("ptfadsreplfailurexml", types[0].Id, "Id was not correct");
            Assert.AreEqual("Active Directory Replication Errors", types[0].Name, "Name was not correct");
            Assert.AreEqual("Checks Windows domain controllers for replication errors", types[0].Description, "Description was not correct");
        }

        [TestMethod]
        public void SensorType_ReadOnly()
        {
            var client = Initialize_ReadOnlyClient(new MultiTypeResponse());

            var result = client.GetSensorTypes(1001).First();

            AssertEx.AllPropertiesRetrieveValues(result);
        }

        [TestMethod]
        public async Task SensorType_ReadOnlyAsync()
        {
            var client = Initialize_ReadOnlyClient(new MultiTypeResponse());

            var result = (await client.GetSensorTypesAsync(1001)).First();

            AssertEx.AllPropertiesRetrieveValues(result);
        }
    }
}
