using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.ObjectTests.Responses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    [TestClass]
    public class DeviceSettingsTests : BaseTest
    {
        [TestMethod]
        public void DeviceSettings_CanDeserialize()
        {
            var client = Initialize_Client(new DeviceSettingsResponse());

            var settings = client.GetDeviceProperties(2212);

            Assert2.AllPropertiesAreNotDefault(settings);
        }
    }
}
