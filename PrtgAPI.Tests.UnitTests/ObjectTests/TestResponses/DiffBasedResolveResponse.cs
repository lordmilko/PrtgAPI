using System;
using System.Linq;
using PrtgAPI.Helpers;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestItems;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses
{
    public class DiffBasedResolveResponse : MultiTypeResponse
    {
        private int start = -1;

        private int[] skip;

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
            var components = UrlHelpers.CrackUrl(address);

            Content content = components["content"].ToEnum<Content>();

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

            int count;

            //1: Before
            //2: After
            //3: Clean
            
            //4: Before
            //5: After
            //6: Clean

            //7: Before
            //8: After
            //9: Clean

            //On the first, third and fourth request (Before, Clean and Before on the next one)
            if (requestCount == 1 || requestCount % 3 == 0 || requestCount % 3 == 1)
                count = 2;
            else
                count = 4;

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
