using System;
using System.Linq;
using PrtgAPI.Utilities;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    public class DiffBasedResolveResponse : MultiTypeResponse
    {
        private int start = -1;

        private int[] skip;

        private bool multiple = true;

        public bool LeadingSpace { get; set; }
        private bool flip;

        private int requestCount;

        private int totalRequestCount;

        public DiffBasedResolveResponse()
        {
            start = 1;
        }

        public DiffBasedResolveResponse(int start)
        {
            this.start = start;
        }

        public DiffBasedResolveResponse(int[] skip)
        {
            this.skip = skip;
        }

        public DiffBasedResolveResponse(bool multiple)
        {
            this.multiple = multiple;
        }

        protected override IWebResponse GetResponse(ref string address, string function)
        {
            switch (function)
            {
                case nameof(XmlFunction.TableData):
                    return GetTableResponse(ref address, function);
            }

            return base.GetResponse(ref address, function);
        }

        private IWebResponse GetTableResponse(ref string address, string function)
        {
            var components = UrlUtilities.CrackUrl(address);

            Content content = components["content"].DescriptionToEnum<Content>();

            totalRequestCount++;

            if (skip != null)
            {
                if (skip.Any(i => i == totalRequestCount))
                    return base.GetResponse(ref address, function);
            }
            else
            {
                if (start > 1)
                {
                    start--;
                    return base.GetResponse(ref address, function);
                }
            }

            requestCount++;

            var leading = LeadingSpace ? " " : "";

            int count;

            //1: Before
            //2: After

            //3: Before
            //4: After

            //5: Before
            //6: After

            //Every odd request (the first request) returns 2; the second request returns 2 
            if (requestCount%2 != 0)
            {
                count = 2;

                if (flip)
                {
                    LeadingSpace = true;
                    flip = false;
                }
            }
            else
            {
                count = multiple ? 4 : 3;

                if (LeadingSpace)
                {
                    flip = true;
                    LeadingSpace = false;
                }
            }

            switch (content)
            {
                case Content.Sensors: return new SensorResponse(GetItems(i => new SensorItem(name: $"{leading}Volume IO _Total{i}",     objid: (1000 + i).ToString()), count));
                case Content.Devices: return new DeviceResponse(GetItems(i => new DeviceItem(name: $"Probe Device{i}",         objid: (1000 + i).ToString()), count));
                case Content.Groups:  return new GroupResponse(GetItems(i => new GroupItem(name: $"Windows Infrastructure{i}", objid: (1000 + i).ToString()), count));
                case Content.Notifications:
                    totalRequestCount--;
                    requestCount--;
                    return new NotificationActionResponse(new NotificationActionItem("301"));
                case Content.Triggers:
                    return new NotificationTriggerResponse(
                        GetItems(
                            i => NotificationTriggerItem.StateTrigger(onNotificationAction: $"301|Email to all members of group PRTG Users Group {i}", subId: i.ToString(), parentId: "1001"),
                            count
                        )
                    );
                case Content.Schedules:
                    totalRequestCount--;
                    requestCount--;
                    return new ScheduleResponse(new ScheduleItem());
                case Content.Channels:
                    return base.GetResponse(ref address, function);
                default:
                    throw new NotImplementedException($"Unknown content '{content}' requested from {nameof(DiffBasedResolveResponse)}");
            }
        }
    }
}
