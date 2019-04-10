using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestItems;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectData
{
    [TestClass]
    public class SensorHistoryTests : BaseTest
    {
        [TestMethod]
        [TestCategory("UnitTest")]
        public void SensorHistory_CanExecute()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            var records = client.GetSensorHistory(1001);

            Assert.AreEqual(2, records.Count);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task SensorHistory_CanExecuteAsync()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            var records = await client.GetSensorHistoryAsync(1001);

            Assert.AreEqual(records.Count, 2);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SensorHistory_CanStream()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            var records = client.StreamSensorHistory(1001).ToList();

            Assert.AreEqual(2, records.Count);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SensorHistory_SetsRawValues()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            var records = client.GetSensorHistory(1001);

            Assert.AreEqual(51, records[0].ChannelRecords[0].Value);
            Assert.AreEqual(1104646144, records[0].ChannelRecords[1].Value);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SensorHistory_CanProcess_TrafficSensorsWithDuplicateChannelNames()
        {
            var channels = new[]
            {
                new SensorHistoryChannelItem(name: "Total (Volume)"),
                new SensorHistoryChannelItem("1", "Total (Speed)"),
                new SensorHistoryChannelItem("2", "Traffic In (Volume) (Yes)"),
                new SensorHistoryChannelItem("3", "Traffic In (Speed) (No)"),
                new SensorHistoryChannelItem("2", "Traffic In (Traffic) (Yes)"),
                new SensorHistoryChannelItem("3", "Traffic In (Traffic) (No)"),
                new SensorHistoryChannelItem("4", "Downtime"),
                new SensorHistoryChannelItem("5", "Channel (Lookup)")
            };

            var client = Initialize_Client(new SensorHistoryResponse(
                new SensorHistoryItem(channels: channels),
                new SensorHistoryItem(channels: channels)));

            var records = client.GetSensorHistory(1001);

            foreach (var record in records)
            {
                Assert.AreEqual("Total(Volume)",           record.ChannelRecords[0].Name);
                Assert.AreEqual("Total(Speed)",            record.ChannelRecords[1].Name);
                Assert.AreEqual("TrafficIn(Volume)",       record.ChannelRecords[2].Name);
                Assert.AreEqual("TrafficIn(Speed)",        record.ChannelRecords[3].Name);
                Assert.AreEqual("TrafficIn(Traffic)(Yes)", record.ChannelRecords[4].Name);
                Assert.AreEqual("TrafficIn(Traffic)(No)",  record.ChannelRecords[5].Name);
                Assert.AreEqual("Downtime",                record.ChannelRecords[6].Name);
                Assert.AreEqual("Channel",                 record.ChannelRecords[7].Name);
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SensorHistory_NoData_AllLanguages()
        {
            AssertEx.AssertErrorResponseAllLanguages<PrtgRequestException>(
                "Not enough monitoring data",
                "Zu wenige Monitoringdaten",
                "監視データが不十分です",
                "The server responded with the following error",
                c => c.GetSensorHistory(1001)
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task SensorHistory_NoData_AllLanguagesAsync()
        {
            await AssertEx.AssertErrorResponseAllLanguagesAsync<PrtgRequestException>(
                "Not enough monitoring data",
                "Zu wenige Monitoringdaten",
                "監視データが不十分です",
                "The server responded with the following error",
                async c => await c.GetSensorHistoryAsync(1001)
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SensorHistory_ReadOnly()
        {
            var client = Initialize_ReadOnlyClient(new MultiTypeResponse());

            var history = client.GetSensorHistory(1001).First();

            AssertEx.AllPropertiesRetrieveValues(history);

            Assert.IsTrue(history.ChannelRecords.Count > 0);

            var channel = history.ChannelRecords.First();

            AssertEx.AllPropertiesRetrieveValues(channel);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task SensorHistory_ReadOnlyAsync()
        {
            var client = Initialize_ReadOnlyClient(new MultiTypeResponse());

            var history = (await client.GetSensorHistoryAsync(1001)).First();

            AssertEx.AllPropertiesRetrieveValues(history);

            Assert.IsTrue(history.ChannelRecords.Count > 0);

            var channel = history.ChannelRecords.First();

            AssertEx.AllPropertiesRetrieveValues(channel);
        }
    }
}
