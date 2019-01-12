using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.IntegrationTests.ObjectData
{
    [TestClass]
    public class GroupTests : BasePrtgClientTest
    {
        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Group_GetGroups_HasAnyResults()
        {
            HasAnyResults(client.GetGroups); //todo - it looks like groups DO have a default priority (3) so why is it reporting null?
            //devices are also null by default

            //todo: maybe assert that sensoritems and sensors have the same number of properties on them, although for sensors
            //we wanna filter for xmlenum/attribute items?
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Group_GetGroups_ReturnsJustGroups()
        {
            ReturnsJustObjectsOfType(client.GetGroups, Settings.Probe, Settings.GroupsInTestProbe - 2, BaseType.Group); //Subtract two for the child groups
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Group_ReadOnlyUser()
        {
            var group = readOnlyClient.GetGroup(Settings.Group);

            AssertEx.AllPropertiesRetrieveValues(group);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public async Task Data_Group_ReadOnlyUserAsync()
        {
            var group = await readOnlyClient.GetGroupAsync(Settings.Group);

            AssertEx.AllPropertiesRetrieveValues(group);
        }
    }
}
