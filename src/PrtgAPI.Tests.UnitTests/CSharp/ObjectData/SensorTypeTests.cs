using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectData
{
    [TestClass]
    public class SensorTypeTests : BaseTest
    {
        [TestMethod]
        [TestCategory("UnitTest")]
        public void SensorType_CanExecute()
        {
            Execute(
                c =>
                {
                    var types = c.GetSensorTypes(3002);

                    Assert.AreEqual(9, types.Count, "Did not return expected number of types");

                    Assert.AreEqual("ptfadsreplfailurexml", types[0].Id, "Id was not correct");
                    Assert.AreEqual("Active Directory Replication Errors", types[0].Name, "Name was not correct");
                    Assert.AreEqual("Checks Windows domain controllers for replication errors", types[0].Description, "Description was not correct");
                },
                UnitRequest.SensorTypes(3002)
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task SensorType_CanExecuteAsync()
        {
            await ExecuteAsync(
                async c =>
                {
                    var types = await c.GetSensorTypesAsync(3002);

                    Assert.AreEqual(9, types.Count, "Did not return expected number of types");

                    Assert.AreEqual("ptfadsreplfailurexml", types[0].Id, "Id was not correct");
                    Assert.AreEqual("Active Directory Replication Errors", types[0].Name, "Name was not correct");
                    Assert.AreEqual("Checks Windows domain controllers for replication errors", types[0].Description, "Description was not correct");
                },
                UnitRequest.SensorTypes(3002)
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SensorType_ReadOnly()
        {
            var client = Initialize_ReadOnlyClient(new MultiTypeResponse());

            var result = client.GetSensorTypes(1001).First();

            AssertEx.AllPropertiesRetrieveValues(result);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task SensorType_ReadOnlyAsync()
        {
            var client = Initialize_ReadOnlyClient(new MultiTypeResponse());

            var result = (await client.GetSensorTypesAsync(1001)).First();

            AssertEx.AllPropertiesRetrieveValues(result);
        }
    }
}
