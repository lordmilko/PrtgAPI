using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectData
{
    [TestClass]
    public class SensorSettingsTests : BaseTest
    {
        [UnitTest]
        [TestMethod]
        public void SensorSettings_AccessesProperties()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            var settings = client.GetSensorProperties(1001);

            foreach (var prop in settings.GetType().GetProperties())
            {
                var val = prop.GetValue(settings);
            }
        }

        [UnitTest]
        [TestMethod]
        public async Task SensorSettings_AccessesPropertiesAsync()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            var settings = await client.GetSensorPropertiesAsync(1001);

            foreach (var prop in settings.GetType().GetProperties())
            {
                var val = prop.GetValue(settings);
            }
        }

        [UnitTest]
        [TestMethod]
        public void SensorSettings_LoadsSchedule_Lazy_AllPropertiesAreSet()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            var validator = new EventValidator(client, new[]
            {
                UnitRequest.SensorProperties(1001),
                UnitRequest.Schedules("filter_objid=627"),
                UnitRequest.ScheduleProperties(623)
            });

            validator.MoveNext();
            var settings = client.GetSensorProperties(1001);

            validator.MoveNext(2);
            var schedule = settings.Schedule;

            AssertEx.AllPropertiesAreNotDefault(schedule, p =>
            {
                if (p.Name == "Tags")
                    return true;

                return false;
            });
        }

        [UnitTest]
        [TestMethod]
        public async Task SensorSettings_LoadsSchedule_Lazy_AllPropertiesAreSetAsync()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            var validator = new EventValidator(client, new[]
            {
                UnitRequest.SensorProperties(1001),
                UnitRequest.Channels(1001),
                UnitRequest.ChannelProperties(1001, 1),
                UnitRequest.Schedules("filter_objid=627"),
                UnitRequest.ScheduleProperties(623)
            });

            validator.MoveNext(5);
            var settings = await client.GetSensorPropertiesAsync(1001);
            Assert.IsTrue(validator.Finished);

            var schedule = settings.Schedule;

            AssertEx.AllPropertiesAreNotDefault(schedule, p =>
            {
                if (p.Name == "Tags")
                    return true;

                return false;
            });
        }

        [UnitTest]
        [TestMethod]
        public void SensorSettings_LoadsChannel_Lazy()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            var validator = new EventValidator(client, new[]
            {
                UnitRequest.SensorProperties(1001),
                UnitRequest.Channels(1001),
                UnitRequest.ChannelProperties(1001, 1)
            });

            validator.MoveNext();
            var settings = client.GetSensorProperties(1001);

            validator.MoveNext(2);
            var channel = settings.PrimaryChannel;

            Assert.IsNotNull(channel);
        }

        [UnitTest]
        [TestMethod]
        public async Task SensorSettings_LoadsChannel_LazyAsync()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            var validator = new EventValidator(client, new[]
            {
                UnitRequest.SensorProperties(1001),
                UnitRequest.Channels(1001),
                UnitRequest.ChannelProperties(1001, 1),
                UnitRequest.Schedules("filter_objid=627"),
                UnitRequest.ScheduleProperties(623)
            });

            validator.MoveNext(5);
            var settings = await client.GetSensorPropertiesAsync(1001);
            Assert.IsTrue(validator.Finished);

            var channel = settings.PrimaryChannel;

            Assert.IsNotNull(channel);
        }

        [UnitTest]
        [TestMethod]
        public void SensorSettings_DoesntLoadChannel_WhenHasNoChannel_Lazy()
        {
            var response = new MultiTypeResponse
            {
                ResponseTextManipulator = (text, address) =>
                {
                    if (address == UnitRequest.SensorProperties(1001))
                        text = SensorSettingsResponse.SetContainerTagContents(text, string.Empty, "select", "primarychannel_");

                    return text;
                }
            };

            var client = Initialize_Client(response);

            var settings = client.GetSensorProperties(1001);

            Assert.IsNull(settings.PrimaryChannel);
        }

        [UnitTest]
        [TestMethod]
        public async Task SensorSettings_DoesntLoadChannel_WhenHasNoChannel_LazyAsync()
        {
            var response = new MultiTypeResponse
            {
                ResponseTextManipulator = (text, address) =>
                {
                    if (address == UnitRequest.SensorProperties(1001))
                        text = SensorSettingsResponse.SetContainerTagContents(text, string.Empty, "select", "primarychannel_");

                    return text;
                }
            };

            var client = Initialize_Client(response);

            var settings = await client.GetSensorPropertiesAsync(1001);

            Assert.IsNull(settings.PrimaryChannel);
        }

        [UnitTest]
        [TestMethod]
        public void SensorSettings_ReadOnly()
        {
            var client = Initialize_ReadOnlyClient(new MultiTypeResponse());

            AssertEx.Throws<InvalidOperationException>(() => client.GetSensorProperties(1001), "Cannot retrieve properties for read-only sensor with ID 1001.");
        }

        [UnitTest]
        [TestMethod]
        public async Task SensorSettings_ReadOnlyAsync()
        {
            var client = Initialize_ReadOnlyClient(new MultiTypeResponse());

            await AssertEx.ThrowsAsync<InvalidOperationException>(async () => await client.GetSensorPropertiesAsync(1001), "Cannot retrieve properties for read-only sensor with ID 1001.");
        }
    }
}
