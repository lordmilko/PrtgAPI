using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.IntegrationTests.ObjectData
{
    public class ScheduleTests : BasePrtgClientTest
    {
        [TestMethod]
        public void Data_Schedule_ReadOnlyUser()
        {
            var schedule = readOnlyClient.GetSchedule(Settings.Schedule);

            AssertEx.AllPropertiesRetrieveValues(schedule);
        }

        [TestMethod]
        public async Task Data_Schedule_ReadOnlyUserAsync()
        {
            var schedule = await readOnlyClient.GetScheduleAsync(Settings.Schedule);

            AssertEx.AllPropertiesRetrieveValues(schedule);
        }
    }
}
