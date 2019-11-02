using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.IntegrationTests.ObjectData
{
    public class ObjectTests : BasePrtgClientTest
    {
        [TestMethod]
        [IntegrationTest]
        public void Data_Object_ReadOnlyUser()
        {
            var objects = readOnlyClient.GetObjects();

            foreach (var obj in objects)
                AssertEx.AllPropertiesRetrieveValues(obj);
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Data_Object_ReadOnlyUserAsync()
        {
            var objects = await readOnlyClient.GetObjectsAsync();

            foreach (var obj in objects)
                AssertEx.AllPropertiesRetrieveValues(obj);
        }
    }
}
