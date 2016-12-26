using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "NotificationAction")]
    public class GetNotificationAction : PrtgObjectCmdlet<NotificationAction>
    {
        protected override IEnumerable<NotificationAction> GetRecords()
        {
            return client.GetNotificationActions();
        }
    }
}
