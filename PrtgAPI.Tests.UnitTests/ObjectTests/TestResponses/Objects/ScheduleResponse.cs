using System.Xml.Linq;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestItems;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses
{
    public class ScheduleResponse : BaseResponse<ScheduleItem>
    {
        internal ScheduleResponse(params ScheduleItem[] schedules) : base("schedules", schedules)
        {
        }

        public override XElement GetItem(ScheduleItem item)
        {
            var xml = new XElement("item",
                new XElement("baselink", item.BaseLink),
                new XElement("baselink_raw", item.BaseLinkRaw),
                new XElement("type", item.Type),
                new XElement("type_raw", item.TypeRaw),
                new XElement("active", item.Active),
                new XElement("active_raw", item.ActiveRaw),
                new XElement("objid", item.ObjId),
                new XElement("name", item.Name)
            );

            return xml;
        }
    }
}