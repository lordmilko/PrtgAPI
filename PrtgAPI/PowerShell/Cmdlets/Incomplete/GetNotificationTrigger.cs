using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.Objects.Shared;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "NotificationTrigger")]
    public class GetNotificationTrigger : PrtgObjectCmdlet<NotificationTrigger>
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public PrtgObject ObjectId { get; set; }

        [Parameter(Mandatory = false)]
        public bool? Inherited { get; set; }

        protected override IEnumerable<NotificationTrigger> GetRecords()
        {
            var triggers = client.GetNotificationTriggers(ObjectId.Id);

            if (Inherited == false)
                return triggers.Where(a => a.Inherited == false);

            return triggers;
        }
    }
}
