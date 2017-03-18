using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Parameters
{
    class NotificationTriggerParameters : Parameters
    {
        public NotificationTriggerParameters(int objectId)
        {
            ObjectId = objectId;
            this[Parameter.Content] = Content.Triggers;
            this[Parameter.Columns] = new[] { Property.Content, Property.ObjId };
        }

        public int ObjectId
        {
            get { return (int)this[Parameter.Id]; }
            set { this[Parameter.Id] = value; }
        }

        public Content Content => (Content)this[Parameter.Content];

        public Property[] Properties => (Property[])this[Parameter.Columns];
    }
}
