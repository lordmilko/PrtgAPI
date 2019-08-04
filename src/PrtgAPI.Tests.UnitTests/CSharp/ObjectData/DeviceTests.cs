using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;
using PrtgAPI.Tests.UnitTests.Support.TestItems;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectData
{
    [TestClass]
    public class DeviceTests : QueryableObjectTests<Device, DeviceItem, DeviceResponse>
    {
        [TestMethod]
        [TestCategory("UnitTest")]
        public void Device_CanDeserialize() => Object_CanDeserialize();

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Device_CanDeserializeAsync() => await Object_CanDeserializeAsync();

        [TestMethod]
        [TestCategory("SlowCoverage")]
        [TestCategory("UnitTest")]
#if MSTEST2
        [DoNotParallelize]
#endif
        public void Device_CanStream_Ordered_FastestToSlowest() => Object_CanStream_Ordered_FastestToSlowest();

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Device_GetObjectsOverloads_CanExecute() => Object_GetObjectsOverloads_CanExecute(
            (c1, c2) => new List<Func<int, object>>                              { c1.GetDevice, c2.GetDeviceAsync },
            (c1, c2) => new List<Func<Property, object, object>>                 { c1.GetDevices, c2.GetDevicesAsync },
            (c1, c2) => new List<Func<Property, FilterOperator, string, object>> { c1.GetDevices, c2.GetDevicesAsync },
            (c1, c2) => new List<Func<SearchFilter[], object>>                   { c1.GetDevices, c2.GetDevicesAsync }
        );

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Device_GetObjectsOverloads_Stream_CanExecute() => Object_GetObjectsOverloads_Stream_CanExecute(
            client => client.StreamDevices,
            client => client.StreamDevices,
            client => client.StreamDevices,
            client => client.StreamDevices
        );

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Device_StreamSerially() => Object_SerialStreamObjects(
            c => c.StreamDevices,
            c => c.StreamDevices,
            new DeviceParameters()
        );

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Device_GetObjectsOverloads_Query_CanExecute() => Object_GetObjectsOverloads_Query_CanExecute(
            client => client.QueryDevices,
            client => client.QueryDevices,
            client => client.QueryDevices,
            client => client.QueryDevices
        );

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Device_GetDevice_Throws_WhenNoObjectReturned() => Object_GetSingle_Throws_WhenNoObjectReturned(c => c.GetDevice(1001));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Device_GetDevice_Throws_WhenMultipleObjectsReturned() => Object_GetSingle_Throws_WhenMultipleObjectsReturned(c => c.GetDevice(1001));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Device_ReadOnly()
        {
            var client = Initialize_ReadOnlyClient(GetResponse(new[] { GetItem() }));

            var device = client.GetDevice(1001);

            AssertEx.AllPropertiesRetrieveValues(device);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Device_ReadOnlyAsync()
        {
            var client = Initialize_ReadOnlyClient(GetResponse(new[] { GetItem() }));

            var device = await client.GetDeviceAsync(1001);

            AssertEx.AllPropertiesRetrieveValues(device);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Device_AllFields_HaveValues() => Object_AllFields_HaveValues();

        protected override List<Device> GetObjects(PrtgClient client) => client.GetDevices();

        protected override async Task<List<Device>> GetObjectsAsync(PrtgClient client) => await client.GetDevicesAsync();

        protected override IEnumerable<Device> Stream(PrtgClient client) => client.StreamDevices();

        public override DeviceItem GetItem() => new DeviceItem();

        protected override DeviceResponse GetResponse(DeviceItem[] items) => new DeviceResponse(items);
    }
}
