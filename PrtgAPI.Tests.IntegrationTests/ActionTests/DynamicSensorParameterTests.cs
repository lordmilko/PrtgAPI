using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Helpers;

namespace PrtgAPI.Tests.IntegrationTests.ActionTests
{
    [TestClass]
    public class DynamicSensorParameterTests : BasePrtgClientTest
    {
        [TestMethod]
        public void DynamicSensorParameters_Adds_ObjectWithTargets()
        {
            var parameters = client.GetDynamicSensorParameters(Settings.Device, "wmivolume");

            AssertEx.AreEqual(1, parameters.Targets.Count, "Did not have expected number of target fields");

            parameters["deviceidlist__check"] = parameters.Targets.First().Value;

            var sensors = client.AddSensor(Settings.Device, parameters);

            try
            {
                AssertEx.AreEqual(3, sensors.Count, "Did not have expected number of sensors");

                AssertEx.IsTrue(sensors[0].Name.Contains("System Reserved"), "Sensor name was incorrect");
                AssertEx.AreEqual("C:\\", sensors[1].Name, "Sensor name was incorrect");
                AssertEx.AreEqual("D:\\", sensors[2].Name, "Sensor name was incorrect");
            }
            finally
            {
                client.RemoveObject(sensors.Select(i => i.Id).ToArray());
            }
        }

        [TestMethod]
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
        public void DynamicSensorParameters_Adds_ObjectWithoutParameters()
        {
            var parameters = client.GetDynamicSensorParameters(Settings.Device, "zen");

            var properties = parameters.GetType().GetNormalProperties().Where(p => p.Name != "Targets");

            foreach (var prop in properties)
            {
                var val = prop.GetValue(parameters);

                if (prop.Name == "Name")
                    Assert.AreEqual("ZEN", val, "Name was not correct");
                else if (prop.Name == "SensorType")
                    Assert.AreEqual("zen", val, "SensorType was not correct");
                else if (prop.Name == "InheritTriggers")
                    Assert.AreEqual(true, val, "InheritTriggers was not correct");
                else
                    Assert.AreEqual(null, val, $"Property {prop.Name} was not null");
            }

            var sensor = client.AddSensor(Settings.Device, parameters).First();

            try
            {
                Assert.AreEqual("ZEN", sensor.Name);
                Assert.AreEqual(sensor.RawType, "zen");
                Assert.AreEqual(sensor.NotificationTypes.InheritTriggers, true);
            }
            finally
            {
                client.RemoveObject(sensor.Id);
            }
        }
    }
}
