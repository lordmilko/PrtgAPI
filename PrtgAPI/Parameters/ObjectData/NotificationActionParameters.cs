using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Parameters
{
    class NotificationActionParameters : ContentParameters<NotificationAction>
    {
        public NotificationActionParameters() : base(Content.Notifications)
        {
        }
    }
}
