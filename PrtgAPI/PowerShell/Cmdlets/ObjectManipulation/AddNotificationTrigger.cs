using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// Add a notification trigger to a PRTG Server.
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "NotificationTrigger", SupportsShouldProcess = true)]
    public class AddNotificationTrigger : BaseSetNotificationTrigger
    {
    }

    /// <summary>
    /// Add or modify a notification trigger on a PRTG Server.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "NotificationTrigger")]
    public class SetNotificationTrigger : BaseSetNotificationTrigger
    {
    }

    /// <summary>
    /// Shared functionality for use in <see cref="AddNotificationTrigger"/> and <see cref="SetNotificationTrigger"/>.
    /// </summary>
    public abstract class BaseSetNotificationTrigger : PrtgCmdlet
    {
        /// <summary>
        /// The parameters to use to add/modify a <see cref="NotificationTrigger"/>.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0)]
        public TriggerParameters Parameters { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if(ShouldProcess($"Object ID: {Parameters.ObjectId} (Type: {Parameters.Type}, Action: {Parameters.OnNotificationAction})"))
                client.AddNotificationTrigger(Parameters);
        }
    }
}
