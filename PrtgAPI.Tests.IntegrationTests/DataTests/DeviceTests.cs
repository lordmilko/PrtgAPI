using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.IntegrationTests
{
    [TestClass]
    public class DeviceTests : BasePrtgClientTest
    {
        [TestMethod]
        public void Data_Device_GetDevices_HasAnyResults()
        {
            HasAnyResults(client.GetDevices);
        }

        [TestMethod]
        public void Data_Device_GetDevices_ReturnsJustDevices()
        {
            ReturnsJustObjectsOfType(client.GetDevices, Settings.Group, Settings.DevicesInTestGroup, BaseType.Device);
        }
    }
}
