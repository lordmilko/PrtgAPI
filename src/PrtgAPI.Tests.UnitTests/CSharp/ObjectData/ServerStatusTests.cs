using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestItems;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectData
{
    [TestClass]
    public class ServerStatusTests : BaseTest
    {
        [UnitTest]
        [TestMethod]
        public void ServerStatus_CanExecute()
        {
            var client = Initialize_Client(new ServerStatusResponse(GetItem()));

            var result = client.GetStatus();

            AssertEx.AllPropertiesAreNotDefault(result);
        }

        [UnitTest]
        [TestMethod]
        public async Task ServerStatus_CanExecuteAsync()
        {
            var client = Initialize_Client(new ServerStatusResponse(GetItem()));

            var result = await client.GetStatusAsync();

            AssertEx.AllPropertiesAreNotDefault(result);
        }

        [UnitTest]
        [TestMethod]
        public void ServerStatus_CanDeserializeEmpty()
        {
            var item = new ServerStatusItem(
                newMessages: "0",
                newAlarms: "0",
                alarms: "",
                partialAlarms: "",
                ackAlarms: "",
                unusualSens: "",
                upSens: "",
                warnSens: "",
                pausedSens: "",
                unknownSens: "",
                newTickets: "",
                userId: "0",
                userTimeZone: "",
                toDos: "",
                favs: "0",
                clock: "10.11.2017 04:27:34 PM",
                version: "1.2.3.4+",
                backgroundTasks: "0",
                correlationTasks: "0",
                autoDiscoTasks: "0",
                reportTasks: "0",
                editionType: "",
                prtgUpdateAvailable: "false",
                maintExpiryDays: "??",
                trialExpiryDays: "-999999",
                commercialExpiryDays: "",
                overloadProtection: "false",
                clusterType: "",
                clusterNodeName: "",
                isAdminUser: "false",
                readOnlyUser: "",
                ticketUser: "",
                readOnlyAllowAcknowledge: "",
                lowMem: "false",
                activationAlert: "",
                prtgHost: "",
                maxSensorCount: "",
                activated: ""
            );

            var client = Initialize_Client(new ServerStatusResponse(item));

            var result = client.GetStatus();

            //Test accessing each property

            foreach (var prop in result.GetType().GetProperties())
            {
                var value = prop.GetValue(result);
            }
        }

        [UnitTest]
        [TestMethod]
        public void ServerStatus_AlternateValues()
        {
            var item = new ServerStatusItem(commercialExpiryDays: "-999999", clusterNodeName: "Cluster Node \\\"PRTG Network Monitor (Failover)\\\" (Failover Node)");

            var client = Initialize_Client(new ServerStatusResponse(item));

            var result = client.GetStatus();

            Assert.AreEqual(null, result.CommercialExpiryDays);
            Assert.AreEqual("PRTG Network Monitor (Failover)", result.ClusterNodeName);
        }

        [UnitTest]
        [TestMethod]
        public void ServerStatus_ReadOnly()
        {
            var client = Initialize_ReadOnlyClient(new MultiTypeResponse());

            var result = client.GetStatus();

            AssertEx.AllPropertiesRetrieveValues(result);
        }

        [UnitTest]
        [TestMethod]
        public async Task ServerStatus_ReadOnlyAsync()
        {
            var client = Initialize_ReadOnlyClient(new MultiTypeResponse());

            var result = (await client.GetStatusAsync());

            AssertEx.AllPropertiesRetrieveValues(result);
        }

        [UnitTest]
        [TestMethod]
        public void ServerStatus_DateTime_ServerUS_ClientUK()
        {
            //There's a legal mismatch, so we get the dates backwards
            TestClockSmallDate(
                "MM/d/yyyy",
                "en-GB",
                expectedClientMonth: 12,
                expectedClientDay: 1
            );

            //There's an illegal mismatch, so we reparse the DateTime using US heuristics
            TestClockLargeDate(
                "MM/d/yyyy",
                "en-GB",
                expectedClientMonth: 1,
                expectedClientDay: 13
            );
        }

        [UnitTest]
        [TestMethod]
        public void ServerStatus_DateTime_ServerUS_ClientUS()
        {
            TestClockSmallDate(
                "MM/d/yyyy",
                "en-US",
                expectedClientMonth: 1,
                expectedClientDay: 12
            );

            TestClockLargeDate(
                "MM/d/yyyy",
                "en-US",
                expectedClientMonth: 1,
                expectedClientDay: 13
            );
        }

        [UnitTest]
        [TestMethod]
        public void ServerStatus_DateTime_ServerUK_ClientUS()
        {
            //There's a legal mismatch, so we get the dates backwards
            TestClockSmallDate(
                "dd/MM/yyyy",
                "en-US",
                expectedClientMonth: 12,
                expectedClientDay: 1
            );

            //There's an illegal mismatch, so we reparse the DateTime using US heuristics
            TestClockLargeDate(
                "dd/MM/yyyy",
                "en-US",
                expectedClientMonth: 1,
                expectedClientDay: 13
            );
        }

        [UnitTest]
        [TestMethod]
        public void ServerStatus_DateTime_ServerUK_ClientUK()
        {
            TestClockSmallDate(
                "dd/MM/yyyy",
                "en-GB",
                expectedClientMonth: 1,
                expectedClientDay: 12
            );

            TestClockLargeDate(
                "dd/MM/yyyy",
                "en-GB",
                expectedClientMonth: 1,
                expectedClientDay: 13
            );
        }

        private void TestClockSmallDate(
            string serverDateFormat,
            string clientCulture,
            int expectedClientMonth,
            int expectedClientDay)
        {
            var serverTime = new DateTime(2020, 1, 12, 4, 10, 20, DateTimeKind.Utc).ToLocalTime().ToString($"{serverDateFormat} h:mm:ss tt");

            TestClock(serverTime, clientCulture, new DateTime(2020, expectedClientMonth, expectedClientDay, 4, 10, 20, DateTimeKind.Utc));
        }

        private void TestClockLargeDate(
            string serverDateFormat,
            string clientCulture,
            int expectedClientMonth,
            int expectedClientDay)
        {
            var serverTime = new DateTime(2020, 1, 13, 4, 10, 20, DateTimeKind.Utc).ToLocalTime().ToString($"{serverDateFormat} h:mm:ss tt");

            TestClock(serverTime, clientCulture, new DateTime(2020, expectedClientMonth, expectedClientDay, 4, 10, 20, DateTimeKind.Utc));
        }

        private void TestClock(string serverTime, string clientCulture, DateTime expectedInvariantUtc)
        {
            var client = Initialize_Client(new ServerStatusResponse(new ServerStatusItem(clock: serverTime)));

            TestCustomCulture(() =>
            {
                var now = DateTime.Now.ToString();

                var status = client.GetStatus();
                Assert.AreEqual(expectedInvariantUtc, status.DateTime.ToUniversalTime());
            }, CultureInfo.GetCultureInfo(clientCulture));
        }

        public ServerStatusItem GetItem() => new ServerStatusItem();
    }
}
