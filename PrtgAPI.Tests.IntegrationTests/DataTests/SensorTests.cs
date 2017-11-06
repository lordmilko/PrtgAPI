using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters.ObjectData;

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

        [TestMethod]
        public void Data_GetSensors_WithParameters_FiltersByStatus()
        {
            var parameters = new SensorParameters();

            //Test an empty value can be retrieved
            var status = parameters.Status;
            Assert2.IsTrue(status == null, "Status was not null");

            //Test a value can be set
            parameters.Status = new[] {Status.Up};
            Assert2.IsTrue(parameters.Status.Length == 1 && parameters.Status.First() == Status.Up, "Status was not up");

            //Test a value can be overwritten
            parameters.Status = new[] { Status.Down };
            Assert2.IsTrue(parameters.Status.Length == 1 && parameters.Status.First() == Status.Down, "Status was not down");

            var sensors = client.GetSensors(parameters);

            Assert2.AreEqual(1, sensors.Count, "Did not contain expected number of down sensors");
            Assert2.AreEqual(Settings.DownSensor, sensors.First().Id, "ID of down sensor was not correct");
        }
    }
}
