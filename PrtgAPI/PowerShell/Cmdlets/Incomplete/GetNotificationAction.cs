using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// Retrieve all Notification Actions from a PRTG Server.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "NotificationAction")]
    public class GetNotificationAction : PrtgObjectCmdlet<NotificationAction>
    {
        /// <summary>
        /// Retrieves all notification actions from a PRTG Server.
        /// </summary>
        /// <returns>A list of all notification actions.</returns>
        protected override IEnumerable<NotificationAction> GetRecords()
        {
            return client.GetNotificationActions();
        }
    }
}
