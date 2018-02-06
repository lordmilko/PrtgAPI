using System.Xml.Linq;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestItems;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses
{
    public class NotificationActionResponse : BaseResponse<NotificationActionItem>
    {
        internal NotificationActionResponse(params NotificationActionItem[] notifications) : base("notifications", notifications)
        {
        }

        public override XElement GetItem(NotificationActionItem item)
        {
            var xml = new XElement("item",
                new XElement("objid", item.ObjId),
                new XElement("name", item.Name)
            );

            return xml;
        }
    }
}
