using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets.Incomplete
{
    [Cmdlet(VerbsCommon.Remove, "NotificationTrigger")]
    class RemoveNotificationTrigger : PrtgCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public NotificationTrigger Trigger { get; set; }

        protected override void ProcessRecord()
        {
            if (Trigger.Inherited)
                throw new InvalidOperationException($"Cannot remove trigger {Trigger.SubId} from object {Trigger.ObjectId} as it is inherited from object {Trigger.ParentId}");

            client.RemoveNotificationTrigger(Trigger.ObjectId, Trigger.SubId);
        }
    }
}
