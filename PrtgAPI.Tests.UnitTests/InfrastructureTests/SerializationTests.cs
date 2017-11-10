using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestItems;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses;

namespace PrtgAPI.Tests.UnitTests.InfrastructureTests
{
    [TestClass]
    public class SerializationTests
    {
        [TestMethod]
        public void Serializer_ThrowsInvalidEnum()
        {
            var webClient = new MockWebClient(new SensorResponse(new SensorItem(status: "banana", statusRaw: "8765")));

            var client = new PrtgClient("prtg.example.com", "username", "password", AuthMode.PassHash, webClient);

            try
            {
                client.GetSensors();
            }
            catch (Exception ex)
            {
                if (ex.Message != "Could not deserialize value '8765' as it is not a valid member of type 'PrtgAPI.Status'. Could not process XML '<status>banana</status><status_raw>8765</status_raw><message><div class=\"status\">OK<div class=\"moreicon\"></div></div></message>'")
                    throw;
            }
        }

        [TestMethod]
        public void ScanningInterval_ParsesCustomTimeSpanSerializedString()
        {
            var interval = ScanningInterval.Parse("20|20 seconds");

            Assert.AreEqual(interval.TimeSpan.TotalSeconds, 20, "TimeSpan was not correct");
        }

        [TestMethod]
        public void ScanningInterval_SerializesTimeSpanCorrectly()
        {
            CheckScanningIntervalSerializedValue(new TimeSpan(0, 0, 1), "1|1 second (Not officially supported)");
            CheckScanningIntervalSerializedValue(new TimeSpan(0, 0, 3), "3|3 seconds (Not officially supported)");
            CheckScanningIntervalSerializedValue(new TimeSpan(0, 0, 30), "30|30 seconds");
            CheckScanningIntervalSerializedValue(new TimeSpan(0, 0, 60), "60|60 seconds");
            CheckScanningIntervalSerializedValue(new TimeSpan(0, 5, 0), "300|5 minutes");
            CheckScanningIntervalSerializedValue(new TimeSpan(0, 10, 0), "600|10 minutes");
            CheckScanningIntervalSerializedValue(new TimeSpan(0, 15, 0), "900|15 minutes");
            CheckScanningIntervalSerializedValue(new TimeSpan(0, 30, 0), "1800|30 minutes");
            CheckScanningIntervalSerializedValue(new TimeSpan(1, 0, 0), "3600|1 hour");
            CheckScanningIntervalSerializedValue(new TimeSpan(4, 0, 0), "14400|4 hours");
            CheckScanningIntervalSerializedValue(new TimeSpan(6, 0, 0), "21600|6 hours");
            CheckScanningIntervalSerializedValue(new TimeSpan(12, 0, 0), "43200|12 hours");
            CheckScanningIntervalSerializedValue(new TimeSpan(24, 0, 0), "86400|24 hours");
            CheckScanningIntervalSerializedValue(new TimeSpan(2, 0, 0, 0), "172800|2 days");
        }

        [TestMethod]
        public void ScanningInterval_SerializesEnumToTimeSpan()
        {
            ParseScanningIntervalAndCheckSerializedValue(StandardScanningInterval.ThirtySeconds, "30|30 seconds");
            ParseScanningIntervalAndCheckSerializedValue(StandardScanningInterval.SixtySeconds, "60|60 seconds");
            ParseScanningIntervalAndCheckSerializedValue(StandardScanningInterval.FiveMinutes, "300|5 minutes");
            ParseScanningIntervalAndCheckSerializedValue(StandardScanningInterval.TenMinutes, "600|10 minutes");
            ParseScanningIntervalAndCheckSerializedValue(StandardScanningInterval.FifteenMinutes, "900|15 minutes");
            ParseScanningIntervalAndCheckSerializedValue(StandardScanningInterval.ThirtyMinutes, "1800|30 minutes");
            ParseScanningIntervalAndCheckSerializedValue(StandardScanningInterval.OneHour, "3600|1 hour");
            ParseScanningIntervalAndCheckSerializedValue(StandardScanningInterval.FourHours, "14400|4 hours");
            ParseScanningIntervalAndCheckSerializedValue(StandardScanningInterval.SixHours, "21600|6 hours");
            ParseScanningIntervalAndCheckSerializedValue(StandardScanningInterval.TwelveHours, "43200|12 hours");
            ParseScanningIntervalAndCheckSerializedValue(StandardScanningInterval.TwentyFourHours, "86400|24 hours");
        }

        [TestMethod]
        public void ScanningInterval_From_ScanningInterval()
        {
            var interval = ScanningInterval.FiveMinutes;

            var newInterval = ScanningInterval.Parse(interval);

            Assert.AreEqual(newInterval.ToString(), interval.ToString());
        }

        [TestMethod]
        public void ScanningInterval_From_Int()
        {
            var interval = ScanningInterval.Parse(300);

            Assert.AreEqual(300, interval.TimeSpan.TotalSeconds);
        }

        [TestMethod]
        public void Object_DeserializesSchedule()
        {
            var webClient = new MockWebClient(new SensorSettingsResponse());

            var client = new PrtgClient("server", "username", "1234567890", AuthMode.PassHash, webClient);

            var properties = client.GetSensorProperties(1001);

            Assert.AreEqual("Weekdays Nights (17:00 - 9:00) [GMT+1100]", properties.Schedule.ToString(), "Schedule was not correct");
        }

        private void CheckScanningIntervalSerializedValue(ScanningInterval interval, string value)
        {
            Assert.AreEqual(((IFormattable) interval).GetSerializedFormat(), value, "Serialized format was not correct");
        }

        private void ParseScanningIntervalAndCheckSerializedValue(StandardScanningInterval interval, string value)
        {
            CheckScanningIntervalSerializedValue(ScanningInterval.Parse(interval), value);
        }
    }
}
