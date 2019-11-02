using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support;
using PrtgAPI.Tests.UnitTests.Support.TestItems;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectData
{
    [TestClass]
    public class NotificationActionTests : StandardObjectTests<NotificationAction, NotificationActionItem, NotificationActionResponse>
    {
        [UnitTest]
        [TestMethod]
        public void NotificationAction_CanDeserialize() => Object_CanDeserialize();

        [UnitTest]
        [TestMethod]
        public async Task NotificationAction_CanDeserializeAsync() => await Object_CanDeserializeAsync();

        [UnitTest]
        [TestMethod]
        public void NotificationAction_AllFields_HaveValues() => Object_AllFields_HaveValues();

        [UnitTest]
        [TestMethod]
        public void NotificationAction_GetObjectsOverloads_CanExecute()
        {
            var client = Initialize_Client_WithItems(GetItem());

            Assert.IsTrue(client.GetNotificationActions().Any());
            Assert.IsTrue(client.GetNotificationActionsAsync().Result.Any());

            Assert.IsTrue(client.GetNotificationActions(Property.Id, 300).Any());
            Assert.IsTrue(client.GetNotificationActionsAsync(Property.Id, 300).Result.Any());

            Assert.IsTrue(client.GetNotificationActions(new SearchFilter(Property.Id, 300)).Any());
            Assert.IsTrue(client.GetNotificationActionsAsync(new SearchFilter(Property.Id, 300)).Result.Any());

            Assert.IsTrue(client.GetNotificationAction(620) != null);
            Assert.IsTrue(client.GetNotificationActionAsync(300).Result != null);

            Assert.IsTrue(client.GetNotificationAction("test") != null);
            Assert.IsTrue(client.GetNotificationActionAsync("test").Result != null);
        }

        [UnitTest]
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

        [UnitTest]
        [TestMethod]
        public void NotificationAction_FiltersByProperty()
        {
            var urls = new[]
            {
                UnitRequest.Notifications("filter_name=ticket"),
                UnitRequest.NotificationProperties(300)
            };

            var countOverride = new Dictionary<Content, int>
            {
                [Content.Notifications] = 1
            };

            Execute(
                c => c.GetNotificationActions(Property.Name, "ticket"),
                urls,
                countOverride
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task NotificationAction_FiltersByPropertyAsync()
        {
            var urls = new[]
            {
                UnitRequest.Notifications("filter_name=ticket"),
                UnitRequest.NotificationProperties(300),
                UnitRequest.Schedules("filter_objid=623"),
                UnitRequest.ScheduleProperties(623)
            };

            var countOverride = new Dictionary<Content, int>
            {
                [Content.Notifications] = 1,
                [Content.Schedules] = 1
            };

            await ExecuteAsync(
                async c => await c.GetNotificationActionsAsync(Property.Name, "ticket"),
                urls,
                countOverride,
                additionalItems: r => r.HasSchedule = new[] {300}
            );
        }
        
        [UnitTest]
        [TestMethod]
        public void NotificationAction_LoadsSchedule_Lazy_AllPropertiesSet()
        {
            var client = Initialize_Client(new NotificationActionResponse(
                new NotificationActionItem()) { HasSchedule = new[] {300}}
            );

            var action = client.GetNotificationAction(300);

            var schedule = action.Schedule;

            AssertEx.AllPropertiesAreNotDefault(schedule, p =>
            {
                if (p.Name == "Tags")
                    return true;

                return false;
            });
        }

        [UnitTest]
        [TestMethod]
        public async Task NotificationAction_LoadsSchedule_Lazy_AllPropertiesSetAsync()
        {
            var client = Initialize_Client(new NotificationActionResponse(
                    new NotificationActionItem())
                { HasSchedule = new[] { 300 } }
            );

            var action = await client.GetNotificationActionAsync(300);

            var schedule = action.Schedule;

            AssertEx.AllPropertiesAreNotDefault(schedule, p =>
            {
                if (p.Name == "Tags")
                    return true;

                return false;
            });
        }

        [UnitTest]
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

        [UnitTest]
        [TestMethod]
        public void NotificationAction_ReadOnly()
        {
            var client = Initialize_ReadOnlyClient(GetResponse(new[] { GetItem() }));

            var action = client.GetNotificationAction(300);

            Assert.IsNull(action.Email);

            AssertEx.AllPropertiesRetrieveValues(action);
        }

        [UnitTest]
        [TestMethod]
        public async Task NotificationAction_ReadOnlyAsync()
        {
            var client = Initialize_ReadOnlyClient(GetResponse(new[] { GetItem() }));

            var action = await client.GetNotificationActionAsync(300);

            Assert.IsNull(action.Email);

            AssertEx.AllPropertiesRetrieveValues(action);
        }

        protected override List<NotificationAction> GetObjects(PrtgClient client) => client.GetNotificationActions();

        protected override Task<List<NotificationAction>> GetObjectsAsync(PrtgClient client) => client.GetNotificationActionsAsync();

        public override NotificationActionItem GetItem() => new NotificationActionItem();

        protected override NotificationActionResponse GetResponse(NotificationActionItem[] items) => new NotificationActionResponse(items);
    }
}
