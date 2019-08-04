using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.IntegrationTests.ObjectManipulation
{
    [TestClass]
    public class PrtgClientStateTests : BasePrtgClientTest
    {
        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Action_State_AcknowledgeAndResume()
        {
            AssertEx.AreEqual(Status.Down, GetSensor(Settings.DownSensor).Status, $"Initial sensor status was not {Status.Down}");

            var message = "Unit Testing FTW!";

            Logger.LogTestDetail("Acknowledging sensor");
            client.AcknowledgeSensor(Settings.DownSensor, 1, message);
            CheckAndSleep(Settings.DownSensor);
            var sensor1 = GetSensor(Settings.DownSensor);
            AssertEx.IsTrue(sensor1.Message.Contains(message), $"Sensor message was {sensor1.Message} instead of {message}");
            AssertEx.AreEqual(Status.DownAcknowledged, sensor1.Status, $"Sensor status was {sensor1.Status} instead of {Status.DownAcknowledged}");

            Logger.LogTestDetail("Waiting 30 seconds for status to update");
            Thread.Sleep(30000);
            client.RefreshObject(Settings.DownSensor);

            var sensor2 = GetSensor(Settings.DownSensor);
            AssertEx.IsTrue(sensor2.Status != Status.DownAcknowledged, $"Sensor status was still {Status.DownAcknowledged}");
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Action_State_PauseAndResume()
        {
            Logger.LogTestDetail("Pausing object");
            client.PauseObject(Settings.UpSensor);
            CheckAndSleep(Settings.UpSensor);
            var sensor1 = GetSensor(Settings.UpSensor);
            AssertEx.IsTrue(sensor1.Status == Status.PausedByUser, $"Sensor status was {sensor1.Status} instead of {Status.PausedByUser}.");

            Logger.LogTestDetail("Resuming object");
            client.ResumeObject(Settings.UpSensor);
            CheckAndSleep(Settings.UpSensor);
            var sensor2 = GetSensor(Settings.UpSensor);

            if (sensor2.Status == Status.PausedByUser)
            {
                var log = client.GetLogs(Settings.UpSensor, RecordAge.Today, 1);

                if (log.Any(l => l.Status == LogStatus.Resuming))
                {
                    Logger.LogTestDetail($"Status was still {Status.PausedByUser}, however a Resume request was received. Waiting 2 minutes");

                    Thread.Sleep(120000);

                    sensor2 = GetSensor(Settings.UpSensor);
                }
            }

            AssertEx.IsTrue(sensor2.Status != Status.PausedByUser, $"Sensor status was still {Status.PausedByUser}.");
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Action_State_PauseForDuration()
        {
            var initial = GetSensor(Settings.UpSensor);

            AssertEx.AreEqual(Status.Up, initial.Status, "Initial status was not up");

            Logger.LogTestDetail("Pausing for 1 minute");
            client.PauseObject(Settings.UpSensor, 1);

            CheckAndSleep(Settings.UpSensor);

            var sensor1 = GetSensor(Settings.UpSensor);
            AssertEx.AreEqual(Status.PausedUntil, sensor1.Status, "Sensor did not pause properly");

            Logger.LogTestDetail("Sleeping for 1 minute");

            Thread.Sleep(60000);

            CheckAndSleep(Settings.UpSensor);

            var sensor2 = GetSensor(Settings.UpSensor);

            if (sensor2.Status != Status.Up)
            {
                Logger.LogTestDetail($"Status was {sensor2.Status}. Waiting up to 5 minutes for sensor to go {Status.Up}");

                int attempts = 10;

                do
                {
                    sensor2 = GetSensor(Settings.UpSensor);

                    if (sensor2.Status != Status.Up)
                    {
                        Thread.Sleep(30000);
                        attempts--;
                    }
                } while (sensor2.Status != Status.Up && attempts > 0);
            }

            AssertEx.AreEqual(Status.Up, sensor2.Status, "Sensor did not unpause properly");
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Action_State_SimulateErrorAndResume()
        {
            AssertEx.AreEqual(Status.Up, GetSensor(Settings.UpSensor).Status, $"Initial sensor state was not {Status.Up}");

            Logger.LogTestDetail("Simulating error status");
            client.SimulateError(Settings.UpSensor);
            CheckAndSleep(Settings.UpSensor);
            var sensor1 = GetSensor(Settings.UpSensor);

            if (sensor1.Status == Status.Up)
            {
                Logger.LogTestDetail($"Status was still {Status.Up}. Waiting 90 seconds");

                CheckAndSleep(Settings.UpSensor);
                CheckAndSleep(Settings.UpSensor);
                CheckAndSleep(Settings.UpSensor);

                sensor1 = GetSensor(Settings.UpSensor);
            }

            AssertEx.IsTrue(sensor1.Status == Status.Down, $"Sensor status was {sensor1.Status} instead of Down.");

            Logger.LogTestDetail("Resuming sensor");
            client.ResumeObject(Settings.UpSensor);
            CheckAndSleep(Settings.UpSensor);
            var sensor2 = GetSensor(Settings.UpSensor);
            AssertEx.IsTrue(sensor2.Status != Status.Down, $"Sensor status was still Down.");
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Action_State_GetPausedSensors_HasDifferentTypes()
        {
            var sensors = client.GetSensors(Status.Paused).Select(s => s.Status).ToList();

            AssertEx.IsTrue(sensors.Any(s => s == Status.PausedByDependency) && sensors.Any(s => s == Status.PausedByUser), "Could not find paused sensors of different types.");
        }

        private Sensor GetSensor(int sensorId)
        {
            var sensor = client.GetSensors(Property.Id, sensorId).FirstOrDefault();

            AssertEx.IsTrue(sensor != null, "GetSensors did not return any results.");

            return sensor;
        }
    }
}
