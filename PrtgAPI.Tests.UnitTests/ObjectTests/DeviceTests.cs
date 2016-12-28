using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.ObjectTests.Support;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    [TestClass]
    public class DeviceTests : ObjectTests<Device, DeviceItem, DeviceResponse>
    {
        [TestMethod]
        public void Device_CanDeserialize() => Object_CanDeserialize();

        [TestMethod]
        public void Device_AllFields_HaveValues() => Object_AllFields_HaveValues();

        protected override List<Device> GetObjects(PrtgClient client) => client.GetDevices();

        protected override DeviceItem GetItem() => new DeviceItem();

        protected override DeviceResponse GetResponse(DeviceItem[] items) => new DeviceResponse(items);
    }
}
