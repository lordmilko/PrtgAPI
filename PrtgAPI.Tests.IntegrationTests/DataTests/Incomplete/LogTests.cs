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
            var count = 6000;

            var logs = client.GetLogs(null, DateTime.Now.AddDays(-1), count: count);

            AssertEx.AreEqual(count, logs.Count, "Did not retrieve expected number of logs");

            var first = logs.First();
            var last = logs.Last();

            AssertEx.AreEqual(DateTime.Now.AddDays(-1).Date, first.DateTime.Date, "Start date was incorrect");
            AssertEx.IsTrue(last.DateTime.Date < DateTime.Now.AddDays(-2), "Logs didn't appear to go back far enough");
        }

        [TestMethod]
        public void Data_Log_GetLogs_SpecifiesEndDate()
        {
            var logs = client.GetLogs(null, endDate: DateTime.Now.AddDays(-1), count: 6000);

            var first = logs.First();
            var last = logs.Last();

            AssertEx.AreEqual(DateTime.Now.Date, first.DateTime.Date, "Start date was incorrect");
            AssertEx.AreEqual(DateTime.Now.AddDays(-1).Date, last.DateTime.Date, "End date was incorrect");
        }

        [TestMethod]
        public void Data_Log_GetLogs_FiltersByStatus()
        {
            var logs = client.GetLogs(status: LogStatus.Up);
            
            foreach (var log in logs)
            {
                AssertEx.AreEqual(log.Status, LogStatus.Up, $"Response included an item that was not {LogStatus.Up}");
            }
        }
    }
}
