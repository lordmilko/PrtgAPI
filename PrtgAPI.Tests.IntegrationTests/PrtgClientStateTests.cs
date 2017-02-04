using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.IntegrationTests
{
    [TestClass]
    public class PrtgClientStateTests : BasePrtgClientTest
    {
        [TestMethod]
        public void Action_State_AcknowledgeAndResume()
        {
            Assert.AreEqual(SensorStatus.Down, GetSensor(Settings.DownSensor).Status, $"Initial sensor status was not {SensorStatus.Down}");

            //bug: because we lowercase everything we've lowercased our Unit Testing FTW text. we need to change how we go about lowercasing things i think
            var message = "Unit Testing FTW!";

            client.AcknowledgeSensor(Settings.DownSensor, 1, message);
            CheckAndSleep(Settings.DownSensor);
            var sensor1 = GetSensor(Settings.DownSensor);
            Assert.IsTrue(sensor1.Message.Contains(message), $"Sensor message was {sensor1.Message} instead of {message}");
            Assert.AreEqual(SensorStatus.DownAcknowledged, sensor1.Status, $"Sensor status was {sensor1.Status} instead of {SensorStatus.DownAcknowledged}");

            Thread.Sleep(30000);
            client.CheckNow(Settings.DownSensor);

            var sensor2 = GetSensor(Settings.DownSensor);
            Assert.IsTrue(sensor2.Status != SensorStatus.DownAcknowledged, $"Sensor status was still {SensorStatus.DownAcknowledged}");
        }

        [TestMethod]
        public void Action_State_PauseAndResume()
        {
            client.Pause(Settings.UpSensor);
            CheckAndSleep(Settings.UpSensor);
            var sensor1 = GetSensor(Settings.UpSensor);
            Assert.IsTrue(sensor1.Status == SensorStatus.PausedByUser, $"Sensor status was {sensor1.Status} instead of {SensorStatus.PausedByUser}.");

            client.Resume(Settings.UpSensor);
            CheckAndSleep(Settings.UpSensor);
            var sensor2 = GetSensor(Settings.UpSensor);
            Assert.IsTrue(sensor2.Status != SensorStatus.PausedByUser, $"Sensor status was still {SensorStatus.PausedByUser}.");
        }

        [TestMethod]
        public void Action_State_SimulateErrorAndResume()
        {
            Assert.AreEqual(SensorStatus.Up, GetSensor(Settings.UpSensor).Status, $"Initial sensor state was not {SensorStatus.Up}");

            client.SimulateError(Settings.UpSensor);
            CheckAndSleep(Settings.UpSensor);
            var sensor1 = GetSensor(Settings.UpSensor);
            Assert.IsTrue(sensor1.Status == SensorStatus.Down, $"Sensor status was {sensor1.Status} instead of Down.");

            client.Resume(Settings.UpSensor);
            CheckAndSleep(Settings.UpSensor);
            var sensor2 = GetSensor(Settings.UpSensor);
            Assert.IsTrue(sensor2.Status != SensorStatus.Down, $"Sensor status was still Down.");
        }

        [TestMethod]
        public void Action_State_GetPausedSensors_HasDifferentTypes()
        {
            //client.Pause(Settings.DownSensor);
            //client.SimulateError(Settings.UpSensor); //todo: we should have an explicit testping sensor

            //CheckAndSleep(Settings.DownSensor);
            //CheckAndSleep(Settings.UpSensor);

            var sensors = client.GetSensors(SensorStatus.Paused).Select(s => s.Status).ToList();

            Assert.IsTrue(sensors.Any(s => s == SensorStatus.PausedByDependency) && sensors.Any(s => s == SensorStatus.PausedByUser), "Could not find paused sensors of different types.");

            //client.Resume(Settings.DownSensor);
            //client.Resume(Settings.UpSensor);

            //CheckAndSleep(Settings.DownSensor);
            //CheckAndSleep(Settings.UpSensor);
        }

        private Sensor GetSensor(int sensorId)
        {
            var sensor = client.GetSensors(Property.ObjId, sensorId).FirstOrDefault();

            Assert.IsTrue(sensor != null, "GetSensors did not return any results.");

            return sensor;
        }
    }
}
