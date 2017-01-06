using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.Items
{
    public class NotificationActionItem : BaseItem
    {
        public string Name { get; set; }

        internal NotificationActionItem(
            string objid = "300", string name = "Email and push notification to admin")
        {
            ObjId = objid;
            Name = name;
        }
    }
}
