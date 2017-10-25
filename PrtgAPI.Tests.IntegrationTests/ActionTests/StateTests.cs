using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.IntegrationTests
{
    [TestClass]
    public class PrtgClientStateTests : BasePrtgClientTest
    {
        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Action_State_AcknowledgeAndResume()
        {
            Assert2.AreEqual(Status.Down, GetSensor(Settings.DownSensor).Status, $"Initial sensor status was not {Status.Down}");

            //bug: because we lowercase everything we've lowercased our Unit Testing FTW text. we need to change how we go about lowercasing things i think
            var message = "Unit Testing FTW!";

            Logger.LogTestDetail("Acknowledging sensor");
            client.AcknowledgeSensor(Settings.DownSensor, 1, message);
            CheckAndSleep(Settings.DownSensor);
            var sensor1 = GetSensor(Settings.DownSensor);
            Assert2.IsTrue(sensor1.Message.Contains(message), $"Sensor message was {sensor1.Message} instead of {message}");
            Assert2.AreEqual(Status.DownAcknowledged, sensor1.Status, $"Sensor status was {sensor1.Status} instead of {Status.DownAcknowledged}");

            Logger.LogTestDetail("Waiting 30 seconds for status to update");
            Thread.Sleep(30000);
            client.RefreshObject(Settings.DownSensor);

            var sensor2 = GetSensor(Settings.DownSensor);
            Assert2.IsTrue(sensor2.Status != Status.DownAcknowledged, $"Sensor status was still {Status.DownAcknowledged}");
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Action_State_PauseAndResume()
        {
            Logger.LogTestDetail("Pausing object");
            client.PauseObject(Settings.UpSensor);
            CheckAndSleep(Settings.UpSensor);
            var sensor1 = GetSensor(Settings.UpSensor);
            Assert2.IsTrue(sensor1.Status == Status.PausedByUser, $"Sensor status was {sensor1.Status} instead of {Status.PausedByUser}.");

            Logger.LogTestDetail("Resuming object");
            client.ResumeObject(Settings.UpSensor);
            CheckAndSleep(Settings.UpSensor);
            var sensor2 = GetSensor(Settings.UpSensor);
            Assert2.IsTrue(sensor2.Status != Status.PausedByUser, $"Sensor status was still {Status.PausedByUser}.");
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Action_State_PauseForDuration()
        {
            var initial = GetSensor(Settings.UpSensor);

            Assert2.AreEqual(Status.Up, initial.Status, "Initial status was not up");

            Logger.LogTestDetail("Pausing for 1 minute");
            client.PauseObject(Settings.UpSensor, 1);

            //Logger.LogTestDetail("Sleeping for 2 minutes");
            //Thread.Sleep(120000);

            CheckAndSleep(Settings.UpSensor);

            var sensor1 = GetSensor(Settings.UpSensor);
            Assert2.AreEqual(Status.PausedUntil, sensor1.Status, "Sensor did not pause properly");

            Logger.LogTestDetail("Sleeping for 1 minute");

            Thread.Sleep(60000);

            CheckAndSleep(Settings.UpSensor);

            var sensor2 = GetSensor(Settings.UpSensor);

            Assert2.AreEqual(Status.Up, sensor2.Status, "Sensor did not unpause properly");
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Action_State_SimulateErrorAndResume()
        {
            Assert2.AreEqual(Status.Up, GetSensor(Settings.UpSensor).Status, $"Initial sensor state was not {Status.Up}");

            Logger.LogTestDetail("Simulating error status");
            client.SimulateError(Settings.UpSensor);
            CheckAndSleep(Settings.UpSensor);
            var sensor1 = GetSensor(Settings.UpSensor);
            Assert2.IsTrue(sensor1.Status == Status.Down, $"Sensor status was {sensor1.Status} instead of Down.");

            Logger.LogTestDetail("Resuming sensor");
            client.ResumeObject(Settings.UpSensor);
            CheckAndSleep(Settings.UpSensor);
            var sensor2 = GetSensor(Settings.UpSensor);
            Assert2.IsTrue(sensor2.Status != Status.Down, $"Sensor status was still Down.");
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Action_State_GetPausedSensors_HasDifferentTypes()
        {
            //client.Pause(Settings.DownSensor);
            //client.SimulateError(Settings.UpSensor); //todo: we should have an explicit testping sensor

            //CheckAndSleep(Settings.DownSensor);
            //CheckAndSleep(Settings.UpSensor);

            var sensors = client.GetSensors(Status.Paused).Select(s => s.Status).ToList();

            Assert2.IsTrue(sensors.Any(s => s == Status.PausedByDependency) && sensors.Any(s => s == Status.PausedByUser), "Could not find paused sensors of different types.");

            //client.Resume(Settings.DownSensor);
            //client.Resume(Settings.UpSensor);

            //CheckAndSleep(Settings.DownSensor);
            //CheckAndSleep(Settings.UpSensor);
        }

        private Sensor GetSensor(int sensorId)
        {
            var sensor = client.GetSensors(Property.Id, sensorId).FirstOrDefault();

            Assert2.IsTrue(sensor != null, "GetSensors did not return any results.");

            return sensor;
        }
    }
}
