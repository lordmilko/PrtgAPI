using System;
using PrtgAPI.Helpers;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestItems;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses
{
    public class DiffBasedResolveResponse : MultiTypeResponse
    {
        private int requestCount;

        protected override IWebResponse GetResponse(ref string address, string function)
        {
            switch (function)
            {
                case nameof(XmlFunction.TableData):
                    return GetTableResponse(address, function);
            }

            return base.GetResponse(ref address, function);
        }

        private IWebResponse GetTableResponse(string address, string function)
        {
            var components = UrlHelpers.CrackUrl(address);

            Content content = components["content"].ToEnum<Content>();

            requestCount++;

            var count = requestCount == 1 ? 2 : 3;

            switch (content)
            {
                case Content.Sensors: return new SensorResponse(GetItems(i => new SensorItem(name: $"Volume IO _Total{i}",     objid: (1000 + i).ToString()), count));
                case Content.Devices: return new DeviceResponse(GetItems(i => new DeviceItem(name: $"Probe Device{i}",         objid: (1000 + i).ToString()), count));
                case Content.Groups:  return new GroupResponse(GetItems(i => new GroupItem(name: $"Windows Infrastructure{i}", objid: (1000 + i).ToString()), count));
                case Content.Triggers:
                    return new NotificationTriggerResponse(
                        GetItems(
                            i => NotificationTriggerItem.StateTrigger(onNotificationAction: $"301|Email to all members of group PRTG Users Group {i}", subId: i.ToString(), parentId: "1001"),
                            count
                        )
                    );
                default:
                    throw new NotImplementedException($"Unknown content '{content}' requested from {nameof(DiffBasedResolveResponse)}");
            }
        }
    }
}
