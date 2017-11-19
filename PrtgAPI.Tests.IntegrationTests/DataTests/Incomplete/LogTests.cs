using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.IntegrationTests.DataTests
{
    [TestClass]
    public class LogTests : BasePrtgClientTest
    {
        [TestMethod]
        public void Data_Log_GetLogs_HasAnyResults()
        {
            HasAnyResults(() => client.GetLogs());
        }

        [TestMethod]
        public void Data_Log_GetLogs_SpecifiesStartDate()
        {
            var count = 5000;

            var logs = client.GetLogs(null, DateTime.Now.AddDays(-1), count: count);

            Assert2.AreEqual(count, logs.Count, "Did not retrieve expected number of logs");

            var first = logs.First();
            var last = logs.Last();

            Assert2.AreEqual(DateTime.Now.AddDays(-1).Date, first.DateTime.Date, "Start date was incorrect");
            Assert2.IsTrue(last.DateTime.Date < DateTime.Now.AddDays(-2), "Logs didn't appear to go back far enough");
        }

        [TestMethod]
        public void Data_Log_GetLogs_SpecifiesEndDate()
        {
            var logs = client.GetLogs(null, endDate: DateTime.Now.AddDays(-1), count: 5000);

            var first = logs.First();
            var last = logs.Last();

            Assert2.AreEqual(DateTime.Now.Date, first.DateTime.Date, "Start date was incorrect");
            Assert2.AreEqual(DateTime.Now.AddDays(-1).Date, last.DateTime.Date, "End date was incorrect");
        }

        [TestMethod]
        public void Data_Log_GetLogs_FiltersByStatus()
        {
            var logs = client.GetLogs(status: LogStatus.Up);
            
            foreach (var log in logs)
            {
                Assert2.AreEqual(log.Status, LogStatus.Up, $"Response included an item that was not {LogStatus.Up}");
            }
        }
    }
}
