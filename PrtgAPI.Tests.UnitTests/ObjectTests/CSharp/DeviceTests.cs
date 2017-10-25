using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestItems;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    [TestClass]
    public class DeviceTests : StreamableObjectTests<Device, DeviceItem, DeviceResponse>
    {
        [TestMethod]
        public void Device_CanDeserialize() => Object_CanDeserialize();

        [TestMethod]
        public async Task Device_CanDeserializeAsync() => await Object_CanDeserializeAsync();

        [TestMethod]
        [TestCategory("SlowCoverage")]
        public void Device_CanStream_Ordered_FastestToSlowest() => Object_CanStream_Ordered_FastestToSlowest();

        [TestMethod]
        [TestCategory("SlowCoverage")]
        public void Device_GetObjectsOverloads_CanExecute() => Object_GetObjectsOverloads_CanExecute(
            (c1, c2, c3) => new List<Func<Property, object, object>>                 { c1.GetDevices, c2.GetDevicesAsync, c3.StreamDevices },
            (c1, c2, c3) => new List<Func<Property, FilterOperator, string, object>> { c1.GetDevices, c2.GetDevicesAsync, c3.StreamDevices },
            (c1, c2, c3) => new List<Func<SearchFilter[], object>>                   { c1.GetDevices, c2.GetDevicesAsync, c3.StreamDevices }
        );

        [TestMethod]
        public void Device_AllFields_HaveValues() => Object_AllFields_HaveValues();

        protected override List<Device> GetObjects(PrtgClient client) => client.GetDevices();

        protected override async Task<List<Device>> GetObjectsAsync(PrtgClient client) => await client.GetDevicesAsync();

        protected override IEnumerable<Device> Stream(PrtgClient client) => client.StreamDevices();

        public override DeviceItem GetItem() => new DeviceItem();

        protected override DeviceResponse GetResponse(DeviceItem[] items) => new DeviceResponse(items);
    }
}
