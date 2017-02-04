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
    /// Remove a notification trigger from a PRTG Object.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "NotificationTrigger", SupportsShouldProcess = true)]
    public class RemoveNotificationTrigger : PrtgCmdlet
    {
        /// <summary>
        /// Notification trigger to remove.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, HelpMessage = "The notification trigger to remove.")]
        public NotificationTrigger Trigger { get; set; }

        /// <summary>
        /// Forces a notification trigger to be removed without displaying a confirmation prompt.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Remove the notification trigger without prompting for confirmation.")]
        public SwitchParameter Force { get; set; }

        /// <summary>
        /// Provides a record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            if (ShouldProcess($"'{Trigger.OnNotificationAction}' (Object ID: {Trigger.ObjectId}, Sub ID: {Trigger.SubId})"))
            {
                if (Force.IsPresent || ShouldContinue($"Are you sure you want to delete notification trigger '{Trigger.OnNotificationAction}' (Object ID: {Trigger.ObjectId}, Sub ID: {Trigger.SubId})", "WARNING!"))
                    client.RemoveNotificationTrigger(Trigger);
            }
        }
    }
}
