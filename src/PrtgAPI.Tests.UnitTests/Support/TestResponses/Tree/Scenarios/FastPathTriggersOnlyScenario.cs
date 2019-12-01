using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class FastPathTriggersOnlyScenario : TreeScenario
    {
        private bool async;

        public FastPathTriggersOnlyScenario(bool async)
        {
            this.async = async;
        }

        protected override IWebResponse GetResponse(string address)
        {
            switch (requestNum)
            {
                case 1:
                    Assert.AreEqual(UnitRequest.Groups("filter_objid=0"), address);
                    return new GroupResponse(new GroupItem(objid: "0", name: "Root", notifiesx: "Inherited 1 State"));

                case 2:
                    Assert.AreEqual(UnitRequest.Triggers(0), address);
                    return new NotificationTriggerResponse(NotificationTriggerItem.StateTrigger());

                case 3:
                    if (!async)
                        goto default;
                    Assert.AreEqual(UnitRequest.Notifications("filter_objid=301"), address);
                    return new NotificationActionResponse(new NotificationActionItem());

                case 4:
                    if (!async)
                        goto default;
                    Assert.AreEqual(UnitRequest.NotificationProperties(301), address);
                    return new NotificationActionResponse(new NotificationActionItem());

                case 5:
                    if (!async)
                        goto default;
                    Assert.AreEqual(UnitRequest.Schedules(), address);
                    return new ScheduleResponse(new ScheduleItem());

                case 6:
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