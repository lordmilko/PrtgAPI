using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;
using PrtgAPI.Tests.IntegrationTests.ActionTests.Types;

namespace PrtgAPI.Tests.IntegrationTests.ActionTests
{
    [TestClass]
    public class AddSensorTests : BasePrtgClientTest
    {
        [TestMethod]
        public void AddSensor_AddsWithRawParameters()
        {
            var parameters = new RawSensorParameters("raw c# sensor", "exexml", Priority.Four, false);
            parameters.Parameters.AddRange(
                new List<CustomParameter>
                {
                    new CustomParameter("tags_", "xmlexesensor"),
                    new CustomParameter("exefile_", "test.ps1|test.ps1||"),
                    new CustomParameter("exeparams_", "arg1 arg2 arg3"),
                    new CustomParameter("environment_", 1),
                    new CustomParameter("usewindowsauthentication_", 1),
                    new CustomParameter("mutexname_", "testMutex"),
                    new CustomParameter("timeout_", 70),
                    new CustomParameter("writeresult_", 1),
                    new CustomParameter("intervalgroup", 0),
                    new CustomParameter("interval_", "30|30 seconds"),
                    new CustomParameter("errorintervalsdown_", 2),
                }
            );

            AddAndValidateRawParameters(parameters);
        }

        [TestMethod]
        public void AddSensor_AddsWithTypedRawParameters()
        {
            var parameters = new ExeXmlRawSensorParameters("raw c# sensor", "exexml", "test.ps1")
            {
                Priority = Priority.Four,
                ExeParameters = "arg1 arg2 arg3",
                SetExeEnvironmentVariables = true,
                UseWindowsAuthentication = true,
                Mutex = "testMutex",
                Timeout = 70,
                DebugMode = DebugMode.WriteToDisk,
                InheritInterval = false,
                Interval = ScanningInterval.ThirtySeconds,
                IntervalErrorMode = IntervalErrorMode.TwoWarningsThenDown,
                InheritTriggers = false
            };

            AddAndValidateRawParameters(parameters);
        }

        private void AddAndValidateRawParameters(RawSensorParameters parameters)
        {
            var sensors = client.GetSensors();

            client.AddSensor(Settings.Device, parameters);

            var newSensors = client.GetSensors();

            Assert2.IsTrue(newSensors.Count > sensors.Count, "New sensor was not added properly");

            var newSensor = newSensors.Where(s => s.Name == "raw c# sensor").ToList();

            Assert2.AreEqual(1, newSensor.Count, "A copy of the new sensor already exists");

            try
            {
                var properties = client.GetSensorProperties(newSensor.First().Id);

                Assert2.AreEqual(properties.Name, "raw c# sensor", "Name was not correct");
                Assert2.AreEqual(properties.Tags.First(), new[] {"xmlexesensor"}.First(), "Tags was not correct");
                Assert2.AreEqual(properties.Priority, Priority.Four, "Priority was not correct");
                Assert2.AreEqual(properties.ExeFile, "test.ps1", "ExeFile was not correct");
                Assert2.AreEqual(properties.ExeParameters, "arg1 arg2 arg3", "ExeParameters was not correct");
                Assert2.AreEqual(properties.SetExeEnvironmentVariables, true, "SetExeEnvironmentVariables was not correct");
                Assert2.AreEqual(properties.UseWindowsAuthentication, true, "UseWindowsAuthentication was not correct");
                Assert2.AreEqual(properties.Mutex, "testMutex", "Mutex was not correct");
                Assert2.AreEqual(properties.Timeout, 70, "Timeout was not correct");
                Assert2.AreEqual(properties.DebugMode, DebugMode.WriteToDisk, "DebugMode was not correct");
                Assert2.AreEqual(properties.InheritInterval, false, "InheritInterval was not correct");
                Assert2.AreEqual(properties.Interval.TimeSpan, new TimeSpan(0, 0, 30), "Interval was not correct");
                Assert2.AreEqual(properties.IntervalErrorMode, IntervalErrorMode.TwoWarningsThenDown, "IntervalErrorMode was not correct");
                Assert2.AreEqual(newSensor.First().NotificationTypes.InheritTriggers, false, "InheritTriggers was not correct");
            }
            finally
            {
                client.RemoveObject(newSensor.First().Id);
            }
        }

        [TestMethod]
        public void AddSensor_Throws_AddingToASensor()
        {
            AddToInvalidObject(Settings.UpSensor);
        }

        [TestMethod]
        public void AddSensor_Throws_AddingToAGroup()
        {
            AddToInvalidObject(Settings.Group);
        }

        [TestMethod]
        public void AddSensor_Throws_AddingToAProbe()
        {
            AddToInvalidObject(Settings.Probe);
        }

        [TestMethod]
        public void AddSensor_Throws_AddingToANonExistentObject()
        {
            var parameters = new ExeXmlSensorParameters("test.ps1");

            try
            {
                client.AddSensor(9995, parameters);
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("The parent object (i.e. device/group) for your newly created sensor doesn't exist anymore"))
                    Assert2.Fail(ex.Message);
            }
        }

        private void AddToInvalidObject(int objectId)
        {
            var parameters = new ExeXmlSensorParameters("test.ps1");

            try
            {
                client.AddSensor(objectId, parameters);
                Assert2.Fail("Expected an exception to be thrown");
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("The desired object cannot be created here"))
                    Assert2.Fail(ex.Message);
            }
        }
    }
}
