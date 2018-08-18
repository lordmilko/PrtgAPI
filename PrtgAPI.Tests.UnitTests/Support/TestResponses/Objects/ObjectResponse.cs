using System;
using System.Xml.Linq;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    public class ObjectResponse : BaseResponse<BaseItem>
    {
        public ObjectResponse(params BaseItem[] items) : base("objects", items)
        {
        }

        public override XElement GetItem(BaseItem item)
        {
            if (item is SensorItem)
                return new SensorResponse().GetItem((SensorItem)item);

            if (item is DeviceItem)
                return new DeviceResponse().GetItem((DeviceItem)item);

            if (item is GroupItem)
                return new GroupResponse().GetItem((GroupItem)item);

            if (item is ProbeItem)
                return new ProbeResponse().GetItem((ProbeItem) item);

            if (item is ScheduleItem)
                return new ScheduleResponse().GetItem((ScheduleItem)item);

            if (item is NotificationActionItem)
                return new NotificationActionResponse().GetItem((NotificationActionItem)item);

            throw new NotImplementedException($"Don't know how to get item of type {item.GetType().Name}");
        }
    }
}
