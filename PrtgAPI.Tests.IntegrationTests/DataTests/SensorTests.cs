using System;
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

            Assert2.AreEqual(Settings.SensorsInTestServer, sensors.Count, "Server did not contain expected number of sensors");
        }

        [TestMethod]
        public void Data_Sensor_GetSensors_WithFilters_ResultsMatch()
        {
            var str = "disk";

            var sensors = client.GetSensors(Property.Name, FilterOperator.Contains, str);

            Assert2.IsTrue(sensors.TrueForAll(s => s.Name.IndexOf(str, StringComparison.OrdinalIgnoreCase) >= 0), $"One or more object names did not include the substring '{str}'");
        }

        [TestMethod]
        public void Data_Sensor_GetSensors_ReturnsJustSensors()
        {
            ReturnsJustObjectsOfType(client.GetSensors, Settings.Device, Settings.SensorsInTestDevice, BaseType.Sensor);
        }
    }
}
