using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class SlowPathTriggersOnlyScenario : TreeScenario
    {
        private bool async;

        public SlowPathTriggersOnlyScenario(bool async)
        {
            this.async = async;
        }

        protected override IWebResponse GetResponse(string address)
        {
            switch (requestNum)
            {
                case 1:
                    Assert.AreEqual(UnitRequest.Objects("filter_objid=1001"), address);
                    return new ProbeResponse(new ProbeItem(objid: "1001"));

                case 2:
                    Assert.AreEqual(UnitRequest.Probes("filter_objid=1001&filter_parentid=0"), address);
                    return new ProbeResponse(new ProbeItem(objid: "1001", notifiesx: "Inherited 1 State"));

                case 3:
                    Assert.AreEqual(UnitRequest.Triggers(1001), address);
                    return new NotificationTriggerResponse(NotificationTriggerItem.StateTrigger(parentId: "1001"));

                case 4:
                    if (!async)
                        goto default;
                    Assert.AreEqual(UnitRequest.Notifications("filter_objid=301"), address);
                    return new NotificationActionResponse(new NotificationActionItem());

                case 5:
                    if (!async)
                        goto default;
                    Assert.AreEqual(UnitRequest.NotificationProperties(301), address);
                    return new NotificationActionResponse(new NotificationActionItem());

                case 6:
                    if (!async)
                        goto default;
                    Assert.AreEqual(UnitRequest.Schedules(), address);
                    return new ScheduleResponse(new ScheduleItem());

                case 7:
                    if (!async)
                        goto default;
                    Assert.AreEqual(UnitRequest.ScheduleProperties(623), address);
                    return new ScheduleResponse(new ScheduleItem());

                default:
                    throw UnknownRequest(address);
            }
        }
    }
}