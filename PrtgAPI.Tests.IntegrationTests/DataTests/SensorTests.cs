using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.IntegrationTests
{
    [TestClass]
    public class SensorTests : BasePrtgClientTest
    {
        [TestMethod]
        public void Data_Sensor_GetSensors_HasExpectedCount()
        {
            var sensors = client.GetSensors();

            Assert.AreEqual(Settings.SensorsInTestServer, sensors.Count);
        }

        [TestMethod]
        public void Data_Sensor_GetSensors_WithFilters_ResultsMatch()
        {
            var str = "disk";

            var sensors = client.GetSensors(Property.Name, FilterOperator.Contains, str);

            Assert.IsTrue(sensors.TrueForAll(s => s.Name.IndexOf(str, StringComparison.OrdinalIgnoreCase) >= 0));
        }

        [TestMethod]
        public void Data_Sensor_GetSensors_ReturnsJustSensors()
        {
            ReturnsJustObjectsOfType(client.GetSensors, Settings.Device, Settings.SensorsInTestDevice, BaseType.Sensor);
        }
    }
}
