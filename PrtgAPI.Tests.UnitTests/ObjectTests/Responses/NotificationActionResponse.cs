using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using PrtgAPI.Tests.UnitTests.ObjectTests.Items;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.Responses
{
    public class NotificationActionResponse : BaseResponse<NotificationActionItem>
    {
        internal NotificationActionResponse(NotificationActionItem[] notifications) : base("notifications", notifications)
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
