using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;
using PrtgAPI.Tests.UnitTests.Support;

namespace PrtgAPI.Tests.IntegrationTests.ObjectManipulation
{
    [TestClass]
    public class DynamicSensorParameterTests : BasePrtgClientTest
    {
        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void DynamicSensorParameters_Adds_ObjectWithTargets()
        {
            var parameters = client.GetDynamicSensorParameters(Settings.Device, "wmivolume");

            AssertEx.AreEqual(1, parameters.Targets.Count, "Did not have expected number of target fields");

            parameters["deviceidlist__check"] = parameters.Targets.First().Value;

            var sensors = client.AddSensor(Settings.Device, parameters).OrderBy(s => s.Name).ToList();

            try
            {
                AssertEx.AreEqual(4, sensors.Count, "Did not have expected number of sensors");

                AssertEx.IsTrue(sensors[0].Name.Contains("System Reserved"), $"Sensor name was incorrect. Actual name: '{sensors[0].Name}'");
                AssertEx.AreEqual("C:\\", sensors[1].Name, "Sensor name was incorrect");
                AssertEx.AreEqual("D:\\ [Storage]", sensors[2].Name, "Sensor name was incorrect");
                AssertEx.AreEqual("E:\\", sensors[3].Name, "Sensor name was incorrect");
            }
            finally
            {
                client.RemoveObject(sensors.Select(i => i.Id).ToArray());
            }
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void DynamicSensorParameters_Gets_ObjectWithoutTargets()
        {
            dynamic parameters = client.GetDynamicSensorParameters(Settings.Device, "http");
            parameters.httpurl = Settings.ServerWithProto;

            Sensor sensor = client.AddSensor(Settings.Device, parameters)[0];

            for (int i = 5; i >= 0; i--)
            {
                if (sensor.Status != Status.Up)
                {
                    CheckAndSleep(sensor.Id);
                    sensor = client.GetSensors(Property.Id, sensor.Id).Single();
                }
                else
                    break;
            }

            try
            {
                AssertEx.AreEqual("HTTP", sensor.Name, "Sensor name was not correct");
                AssertEx.AreEqual(Status.Up, sensor.Status, "Sensor status was not correct");

                var properties = client.GetSensorProperties(sensor.Id);

                AssertEx.AreEqual(Settings.ServerWithProto, properties.Url, "URL was not correct");
            }
            finally
            {
                client.RemoveObject(sensor.Id);
            }
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void DynamicSensorParameters_Adds_ObjectWithoutParameters()
        {
            var parameters = client.GetDynamicSensorParameters(Settings.Device, "zen");

            var properties = parameters.GetType().GetNormalProperties().Where(p => p.Name != nameof(DynamicSensorParameters.Targets) && p.Name != nameof(DynamicSensorParameters.DynamicType));

            foreach (var prop in properties)
            {
                var val = prop.GetValue(parameters);

                switch (prop.Name)
                {
                    case nameof(NewSensorParameters.Name):
                        Assert.AreEqual("ZEN", val, "Name was not correct");
                        break;
                    case nameof(DynamicSensorParameters.SensorType):
                        Assert.AreEqual("zen", val, "SensorType was not correct");
                        break;
                    case nameof(NewSensorParameters.InheritTriggers):
                        Assert.AreEqual(true, val, "InheritTriggers was not correct");
                        break;
                    case nameof(NewSensorParameters.Priority):
                        Assert.AreEqual(Priority.Three, val, "Priority was not correct");
                        break;
                    case nameof(NewSensorParameters.InheritInterval):
                        Assert.AreEqual(true, val, "InheritInterval was not correct");
                        break;
                    case nameof(NewSensorParameters.Interval):
                        Assert.AreEqual(ScanningInterval.SixtySeconds, val, "Interval was not correct");
                        break;
                    case nameof(NewSensorParameters.IntervalErrorMode):
                        Assert.AreEqual(IntervalErrorMode.OneWarningThenDown, val, "IntervalErrorMode was not correct");
                        break;
                    default:
                        Assert.AreEqual(null, val, $"Property {prop.Name} was not null");
                        break;
                }
            }

            var sensor = client.AddSensor(Settings.Device, parameters).First();

            try
            {
                Assert.AreEqual("ZEN", sensor.Name);
                Assert.AreEqual(sensor.Type, "zen");
                Assert.AreEqual(sensor.NotificationTypes.InheritTriggers, true);
                Assert.AreEqual(Priority.None, sensor.Priority); //ZEN sensors don't have a priority
                Assert.AreEqual(false, sensor.InheritInterval);  //ZEN sensors don't have an InheritInterval
                Assert.AreEqual(ScanningInterval.TwentyFourHours, sensor.Interval); //ZEN sensors don't have an Interval. SensorSettings will report this value as null
                Assert.AreEqual(null, client.GetSensorProperties(sensor.Id).IntervalErrorMode); //ZEN sensors don't have an InheritInterval
            }
            finally
            {
                client.RemoveObject(sensor.Id);
            }
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void DynamicSensorParameters_ReadOnlyUser_Throws()
        {
            AssertEx.Throws<PrtgRequestException>(
                () => readOnlyClient.GetDynamicSensorParameters(Settings.Device, "exexml"),
                "type was not valid or you do not have sufficient permissions"
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public async Task DynamicSensorParameters_ReadOnlyUser_ThrowsAsync()
        {
            await AssertEx.ThrowsAsync<PrtgRequestException>(
                async () => await readOnlyClient.GetDynamicSensorParametersAsync(Settings.Device, "exexml"),
                "type was not valid or you do not have sufficient permissions"
            );
        }

        [TestMethod]
        [TestCategory("Unreliable")]
        [TestCategory("IntegrationTest")]
        public void DynamicSensorParameters_Timeout_CustomTimeout()
        {
            AssertEx.Throws<TimeoutException>(
                () => client.GetDynamicSensorParameters(Settings.Device, "exexml", timeout: 0),
                "Failed to retrieve sensor information within a reasonable period of time."
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public async Task DynamicSensorParameters_Timeout_CustomTimeoutAsync()
        {
            await AssertEx.ThrowsAsync<TimeoutException>(
                async () => await client.GetDynamicSensorParametersAsync(Settings.Device, "exexml", timeout: 0),
                "Failed to retrieve sensor information within a reasonable period of time."
            );
        }
    }
}
