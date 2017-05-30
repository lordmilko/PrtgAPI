using System.Xml.Linq;
using PrtgAPI.Tests.UnitTests.ObjectTests.Items;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.Responses
{
    public class NotificationTriggerResponse : BaseResponse<NotificationTriggerItem>
    {
        internal NotificationTriggerResponse(NotificationTriggerItem[] triggers) : base("triggers", triggers)
        {
        }

        public override XElement GetItem(NotificationTriggerItem item)
        {
            var xml = new XElement("item",
                new XElement("content", item.Content),
                new XElement("objid", item.ObjId)
            );

            return xml;
        }
    }
}
