using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;

namespace PrtgAPI.Tests.IntegrationTests.ObjectData
{
    [TestClass]
    public class SensorTests : BasePrtgClientTest
    {
        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Sensor_GetSensors_HasExpectedCount()
        {
            var sensors = client.GetSensors();

            AssertEx.AreEqual(Settings.SensorsInTestServer, sensors.Count, "Server did not contain expected number of sensors");
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Sensor_GetSensors_WithFilters_ResultsMatch()
        {
            var str = "disk";
            
            var sensors = client.GetSensors(Property.Name, FilterOperator.Contains, str);

            AssertEx.IsTrue(sensors.TrueForAll(s => s.Name.IndexOf(str, StringComparison.OrdinalIgnoreCase) >= 0), $"One or more object names did not include the substring '{str}'");
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Sensor_GetSensors_ReturnsJustSensors()
        {
            ReturnsJustObjectsOfType(client.GetSensors, Settings.Device, Settings.SensorsInTestDevice, BaseType.Sensor);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
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
        [TestCategory("IntegrationTest")]
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
        [TestCategory("IntegrationTest")]
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
        [TestCategory("IntegrationTest")]
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
        [TestCategory("IntegrationTest")]
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

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_StreamSensors_StartOffset_CorrectCount()
        {
            var count = 15;

            var parameters = new SensorParameters
            {
                Count = count,
                Start = 3,
                PageSize = 5
            };

            var sensors = client.StreamSensors(parameters).ToList();

            Assert.AreEqual(count, sensors.Count);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_StreamSensors_WithCorrectPageSize()
        {
            LogTests.Stream_WithCorrectPageSize(
                () => client.GetSensors(new SensorParameters
                {
                    Count = 15
                }),
                () => client.StreamSensors(new SensorParameters
                {
                    Count = 15
                }, true),
                p => client.GetSensors(p),
                new PrtgObjectComparer(),
                new SensorParameters
                {
                    Count = 5
                },
                null
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_StreamSensors_WithIncorrectPageSize()
        {
            var normalParameters = new SensorParameters { Count = 15 };
            var manualParameters = new SensorParameters { Start = 1, Count = 5 };
            Assert.AreEqual(1, manualParameters.Start);

            var normalSensors = client.GetSensors(normalParameters);
            var streamedSensors = client.StreamSensors(normalParameters).ToList();

            var firstParamSensors = client.GetSensors(manualParameters);
            manualParameters.Page++;
            var secondParamSensors = client.GetSensors(manualParameters);
            manualParameters.Page++;
            var thirdParamSensors = client.GetSensors(manualParameters);

            var manualSensors = new List<Sensor>();
            manualSensors.AddRange(firstParamSensors);
            manualSensors.AddRange(secondParamSensors);
            manualSensors.AddRange(thirdParamSensors);

            AssertEx.AreEqualLists(normalSensors, streamedSensors, new PrtgObjectComparer(), "Normal and streamed sensors were not equal");

            AssertEx.IsFalse(manualSensors.Contains(normalSensors.First()), "The first normal sensor was contained in manual sensors, however it shouldn't have been");
            AssertEx.IsFalse(normalSensors.Contains(manualSensors.Last()), "The last manual sensor was contained in normal sensors, however it shouldn't have been");

            var middleNormalSensors = normalSensors.Skip(1).ToList();
            var middleManualSensors = manualSensors.Take(manualSensors.Count - 1).ToList();

            AssertEx.AreEqualLists(middleNormalSensors, middleManualSensors, new PrtgObjectComparer(), "The middle of the normal and manual lists were not equal");
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Sensor_ReadOnlyUser()
        {
            var sensor = readOnlyClient.GetSensor(Settings.UpSensor);

            AssertEx.AllPropertiesRetrieveValues(sensor);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public async Task Data_Sensor_ReadOnlyUserAsync()
        {
            var sensor = await readOnlyClient.GetSensorAsync(Settings.UpSensor);

            AssertEx.AllPropertiesRetrieveValues(sensor);
        }
    }
}
