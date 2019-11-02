using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;
using PrtgAPI.Tests.UnitTests;
using PrtgAPI.Tests.UnitTests.Support;

namespace PrtgAPI.Tests.IntegrationTests.ObjectManipulation
{
    [TestClass]
    public class DynamicSensorParameterTests : BasePrtgClientTest
    {
        [TestMethod]
        [IntegrationTest]
        public void Action_DynamicSensorParameters_Adds_ObjectWithTargets()
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
        [IntegrationTest]
        public void Action_DynamicSensorParameters_Gets_ObjectWithoutTargets()
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
        [IntegrationTest]
        public void Action_DynamicSensorParameters_Adds_ObjectWithoutParameters()
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

                if (IsEnglish)
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
        [IntegrationTest(TestCategory.Unreliable)]
        public void Action_DynamicSensorParameters_Timeout_CustomTimeout()
        {
            AssertEx.Throws<TimeoutException>(
                () => client.GetDynamicSensorParameters(Settings.Device, "exexml", timeout: 0),
                "Failed to retrieve sensor information within a reasonable period of time."
            );
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Action_DynamicSensorParameters_Timeout_CustomTimeoutAsync()
        {
            await AssertEx.ThrowsAsync<TimeoutException>(
                async () => await client.GetDynamicSensorParametersAsync(Settings.Device, "exexml", timeout: 0),
                "Failed to retrieve sensor information within a reasonable period of time."
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_DynamicSensorParameters_AsReadOnlyUser_NoQueryTarget_Throws()
        {
            AssertEx.Throws<PrtgRequestException>(
                () => readOnlyClient.GetDynamicSensorParameters(Settings.Device, "exexml"),
                "you may not have sufficient permissions on the specified object. The server responded"
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_DynamicSensorParameters_AsReadOnlyUser_WithQueryTarget_Throws()
        {
            AssertEx.Throws<PrtgRequestException>(
                () => readOnlyClient.GetDynamicSensorParameters(Settings.Device, "snmplibrary", queryParameters: (SensorQueryTarget)"APC UPS.oidlib"),
                "you may not have sufficient permissions on the specified object. The server responded"
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_DynamicSensorParameters_AsReadOnlyUser_WithQueryTargetParameters_Throws()
        {
            AssertEx.Throws<PrtgRequestException>(
                () => readOnlyClient.GetDynamicSensorParameters(Settings.Device, "oracletablespace"),
                "you may not have sufficient permissions on the specified object. The server responded"
            );
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Action_DynamicSensorParameters_AsReadOnlyUser_NoQueryTarget_ThrowsAsync()
        {
            await AssertEx.ThrowsAsync<PrtgRequestException>(
                async () => await readOnlyClient.GetDynamicSensorParametersAsync(Settings.Device, "exexml"),
                "you may not have sufficient permissions on the specified object. The server responded"
            );
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Action_DynamicSensorParameters_AsReadOnlyUser_WithQueryTarget_ThrowsAsync()
        {
            await AssertEx.ThrowsAsync<PrtgRequestException>(
                async () => await readOnlyClient.GetDynamicSensorParametersAsync(Settings.Device, "snmplibrary", queryParameters: (SensorQueryTarget)"APC UPS.oidlib"),
                "you may not have sufficient permissions on the specified object. The server responded"
            );
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Action_DynamicSensorParameters_AsReadOnlyUser_WithQueryTargetParameters_ThrowsAsync()
        {
            await AssertEx.ThrowsAsync<PrtgRequestException>(
                async () => await readOnlyClient.GetDynamicSensorParametersAsync(Settings.Device, "oracletablespace"),
                "you may not have sufficient permissions on the specified object. The server responded"
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_DynamicSensorParameters_SensorQueryTarget_ParsesTarget()
        {
            AssertEx.Throws<TimeoutException>(
                () => client.GetDynamicSensorParameters(Settings.Device, "snmplibrary", timeout: 3, queryParameters: (SensorQueryTarget)"APC UPS.oidlib"),
                "Failed to retrieve sensor information within a reasonable period of time"
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_DynamicSensorParameters_SensorQueryTarget_Throws_WhenTargetIsInvalid()
        {
            AssertEx.Throws<InvalidOperationException>(
                () => client.GetDynamicSensorParameters(Settings.Device, "snmplibrary", timeout: 3, queryParameters: (SensorQueryTarget)"test"),
                $"Query target 'test' is not a valid target for sensor type 'snmplibrary' on device ID {Settings.Device}. Please specify one of the following targets: 'APC UPS.oidlib',"
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_DynamicSensorParameters_SensorQueryTarget_Throws_WhenTargetIsNotRequired()
        {
            AssertEx.Throws<InvalidOperationException>(
                () => client.GetDynamicSensorParameters(Settings.Device, "exexml", queryParameters: (SensorQueryTarget)"test"),
                "Cannot specify query target 'test' on sensor type 'exexml': type does not support query targets."
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_DynamicSensorParameters_SensorQueryTarget_Throws_WhenTargetMissing()
        {
            AssertEx.Throws<InvalidOperationException>(
                () => client.GetDynamicSensorParameters(Settings.Device, "snmplibrary"),
                "Failed to process query for sensor type 'snmplibrary': a sensor query target is required, however none was specified. Please specify one of the following targets: 'APC UPS.oidlib',"
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_DynamicSensorParameters_SensorQueryParameters_ParsesParameters()
        {
            var queryParameters = new SensorQueryTargetParameters
            {
                ["database"] = "XE",
                ["sid_type"] = 0,
                ["prefix"] = 0
            };

            AssertEx.Throws<PrtgRequestException>(
                () => client.GetDynamicSensorParameters(Settings.Device, "oracletablespace", queryParameters: queryParameters),
                "Specified sensor type may not be valid on this device"
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_DynamicSensorParameters_SensorQueryParameters_Throws_WhenParametersAreMissing()
        {
            AssertEx.Throws<InvalidOperationException>(
                () => client.GetDynamicSensorParameters(Settings.Device, "oracletablespace", queryParameters: new SensorQueryTargetParameters()),
                "Failed to process request for sensor type 'oracletablespace': sensor query target parameters did not include mandatory parameters 'database_', 'sid_type_', 'prefix_'."
            );
        }
    }
}
