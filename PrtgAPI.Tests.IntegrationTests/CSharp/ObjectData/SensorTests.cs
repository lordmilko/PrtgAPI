using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;

namespace PrtgAPI.Tests.IntegrationTests.DataTests
{
    [TestClass]
    public class SensorTests : BasePrtgClientTest
    {
        [TestMethod]
        public void Data_Sensor_GetSensors_HasExpectedCount()
        {
            var sensors = client.GetSensors();

            AssertEx.AreEqual(Settings.SensorsInTestServer, sensors.Count, "Server did not contain expected number of sensors");
        }

        [TestMethod]
        public void Data_Sensor_GetSensors_WithFilters_ResultsMatch()
        {
            var str = "disk";

            var sensors = client.GetSensors(Property.Name, FilterOperator.Contains, str);

            AssertEx.IsTrue(sensors.TrueForAll(s => s.Name.IndexOf(str, StringComparison.OrdinalIgnoreCase) >= 0), $"One or more object names did not include the substring '{str}'");
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
            AssertEx.IsTrue(status == null, "Status was not null");

            //Test a value can be set
            parameters.Status = new[] {Status.Up};
            AssertEx.IsTrue(parameters.Status.Length == 1 && parameters.Status.First() == Status.Up, "Status was not up");

            //Test a value can be overwritten
            parameters.Status = new[] { Status.Down };
            AssertEx.IsTrue(parameters.Status.Length == 1 && parameters.Status.First() == Status.Down, "Status was not down");

            //Ignore Probe Health sensor due to a bug in PRTG 17.4.35
            var sensors = client.GetSensors(parameters);

            AssertEx.AreEqual(1, sensors.Count, $"Did not contain expected number of down sensors. Sensors found were: {string.Join(",", sensors)}");
            AssertEx.AreEqual(Settings.DownSensor, sensors.First().Id, "ID of down sensor was not correct");
        }

        [TestMethod]
        public void Data_GetSensors_WithParameters_SortsByProperty()
        {
            var parameters = new SensorParameters {SortBy = Property.Id};
            var ascending = client.GetSensors(parameters);
            var linqAscending = ascending.OrderBy(s => s.Id);
            AssertEx.IsTrue(ascending.SequenceEqual(linqAscending), "Ascending lists were not equal");

            parameters.SortDirection = SortDirection.Descending;
            var descending = client.GetSensors(parameters);
            var linqDescending = descending.OrderByDescending(s => s.Id);
            AssertEx.IsTrue(descending.SequenceEqual(linqDescending), "Descending lists were not equal");

            AssertEx.IsFalse(ascending.SequenceEqual(descending), "Ascending and descending lists were equal");
        }

        [TestMethod]
        public void Data_GetSensors_FiltersByTimeSpan()
        {
            var sensor = client.AddSensor(Settings.Device, new HttpSensorParameters()).Single();

            try
            {
                CheckAndSleep(sensor.Id);

                var newSensor = client.GetSensors(
                    new SearchFilter(Property.UpDuration, FilterOperator.LessThan, TimeSpan.FromMinutes(1)),
                    new SearchFilter(Property.Id, sensor.Id)
                );

                Assert.AreEqual(1, newSensor.Count);
                Assert.AreEqual(sensor.Id, newSensor.Single().Id);
            }
            finally
            {
                client.RemoveObject(sensor.Id);
            }
        }

        [TestMethod]
        public void Data_GetSensors_FiltersByDateTime()
        {
            var sensor = client.AddSensor(Settings.Device, new HttpSensorParameters()).Single();

            try
            {
                CheckAndSleep(sensor.Id);

                var check1 = client.GetSensors(Property.Id, sensor.Id).Single();

                if (check1.LastCheck == null)
                    CheckAndSleep(sensor.Id);

                var check2 = client.GetSensors(Property.Id, sensor.Id).Single();

                var newSensor = client.GetSensors(
                    new SearchFilter(Property.LastCheck, FilterOperator.GreaterThan, DateTime.Now.AddMinutes(-1)),
                    new SearchFilter(Property.Id, sensor.Id)
                );

                Assert.IsTrue(newSensor.Any(s => s.Id == sensor.Id));
            }
            finally
            {
                client.RemoveObject(sensor.Id);
            }
        }

        [TestMethod]
        public void Data_GetSensors_FiltersByBool()
        {
            var sensor = client.GetSensor(Settings.UpSensor);
            
            AssertEx.IsTrue(sensor.Active, "Up sensor was not active");

            var activeSensors = client.GetSensors(Property.Active, false);

            client.PauseObject(Settings.UpSensor);

            CheckAndSleep(Settings.UpSensor);

            try
            {
                var newSensor = client.GetSensor(Settings.UpSensor);
                var newActiveSensors = client.GetSensors(Property.Active, false);

                Assert.IsFalse(newSensor.Active);
                Assert.IsTrue(newActiveSensors.Count > activeSensors.Count);
            }
            finally
            {
                client.ResumeObject(Settings.UpSensor);
            }
        }
    }
}
