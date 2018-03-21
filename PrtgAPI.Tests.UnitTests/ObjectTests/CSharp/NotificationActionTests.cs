using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestItems;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    [TestClass]
    public class NotificationActionTests : ObjectTests<NotificationAction, NotificationActionItem, NotificationActionResponse>
    {
        [TestMethod]
        public void NotificationAction_CanDeserialize() => Object_CanDeserialize();

        [TestMethod]
        public async Task NotificationAction_CanDeserializeAsync() => await Object_CanDeserializeAsync();

        [TestMethod]
        public void NotificationAction_AllFields_HaveValues() => Object_AllFields_HaveValues();

        [TestMethod]
        public void NotificationAction_NotificationTypes_AllFields_HaveValues()
        {
            var obj = GetSingleItem();

            var actions = obj.GetType().GetProperties().Where(p => p.PropertyType.Name.EndsWith("Settings")).Select(p => p.GetValue(obj)).ToList();

            foreach (var action in actions)
            {
                try
                {
                    AssertEx.AllPropertiesAreNotDefault(action);
                }
                catch (AssertFailedException ex)
                {
                    throw new AssertFailedException($"{action.GetType()}: {ex.Message}", ex);
                }
            }
        }

        [TestMethod]
        public void NotificationAction_FiltersByProperty()
        {
            var client = Initialize_Client(new AddressValidatorResponse(new[]
            {
                "https://prtg.example.com/api/table.xml?content=notifications&columns=type,tags,active,objid,name&count=*&filter_name=ticket&username=username&passhash=12345678",
                "https://prtg.example.com/controls/editnotification.htm?id=300&username=username&passhash=12345678"
            }));

            client.GetNotificationActions(Property.Name, "ticket");
        }

        [TestMethod]
        public async Task NotificationAction_FiltersByPropertyAsync()
        {
            var client = Initialize_Client(new AddressValidatorResponse(new[]
            {
                "https://prtg.example.com/api/table.xml?content=notifications&columns=type,tags,active,objid,name&count=*&filter_name=ticket&username=username&passhash=12345678",
                "https://prtg.example.com/controls/editnotification.htm?id=300&username=username&passhash=12345678"
            }));

            await client.GetNotificationActionsAsync(Property.Name, "ticket");
        }
        
        [TestMethod]
        public void NotificationAction_Types_ToString()
        {
            var obj = GetSingleItem();

            Assert.AreEqual("PRTG Users Group, test@example.com", obj.Email.ToString(), "Email was not correct");
            Assert.AreEqual("None", obj.Push.ToString(), "Push was not correct");
            Assert.AreEqual("1234567890", obj.SMS.ToString(), "SMS was not correct");
            Assert.AreEqual("Log: PRTG Network Monitor, Type: Warning", obj.EventLog.ToString(), "EventLog was not correct");
            Assert.AreEqual("localhost:514", obj.Syslog.ToString(), "Syslog was not correct");
            Assert.AreEqual("localhost:162", obj.SNMP.ToString(), "SNMP was not correct");
            Assert.AreEqual("http://localhost", obj.Http.ToString(), "HTTP was not correct");
            Assert.AreEqual("Demo EXE Notification - OutFile.bat", obj.Program.ToString(), "Program was not correct");
            Assert.AreEqual("message subject", obj.Amazon.ToString(), "Amazon was not correct");
            Assert.AreEqual("PRTG System Administrator, PRTG Administrators", obj.Ticket.ToString(), "Ticket was not correcet");
        }

        protected override List<NotificationAction> GetObjects(PrtgClient client) => client.GetNotificationActions();

        protected override Task<List<NotificationAction>> GetObjectsAsync(PrtgClient client) => client.GetNotificationActionsAsync();

        public override NotificationActionItem GetItem() => new NotificationActionItem();

        protected override NotificationActionResponse GetResponse(NotificationActionItem[] items) => new NotificationActionResponse(items);
    }
}
