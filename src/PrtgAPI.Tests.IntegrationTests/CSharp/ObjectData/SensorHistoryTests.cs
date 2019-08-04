using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.IntegrationTests.ObjectData
{
    [TestClass]
    public class SensorHistoryTests : BasePrtgClientTest
    {
        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_SensorHistory_SetsRawValues()
        {
            var history = client.GetSensorHistory(Settings.UpSensor);

            Assert.IsTrue(history.SelectMany(h => h.ChannelRecords).Any(r => r.Value != 0), "All records were missing a raw value");
        }
    }
}
