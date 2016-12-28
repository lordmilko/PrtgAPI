using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.Support
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
