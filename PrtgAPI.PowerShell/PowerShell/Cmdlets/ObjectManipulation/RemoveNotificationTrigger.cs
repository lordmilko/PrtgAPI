using System.Management.Automation;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Removes a notification trigger from a PRTG Object.</para>
    /// 
    /// <para type="description">The Remove-NotificationTrigger cmdlet removes a notification trigger from a PRTG Object.
    /// Notification triggers can only be removed from the objects on which they are explicitly defined. Attempting to
    /// remove a notification trigger from an object which inherits the trigger from its parent will generate an exception.</para>
    /// <para type="description">If invoked with no arguments other than the notification trigger to be deleted, Remove-NotificationTrigger
    /// will prompt for confirmation of each trigger to be deleted. If you wish to remove multiple triggers, it is recommended to
    /// first run Remove-NotificationTrigger with the -WhatIf parameter, and then re-run with the -Force parameter if the results of
    /// -WhatIf look correct</para>
    /// <para type="description">When invoked with -WhatIf, Remove-NotificationTrigger will list all notification triggers that would have been removed,
    /// along with the OnNotificationAction and Sub ID of the notification trigger and the object ID of the object to which the triggers are applied.
    /// Even if you are sure of the triggers you wish to delete, it is recommended to always run with -WhatIf first to confirm you have specified
    /// the correct triggers and that PrtgAPI has interpreted your request in the way you intended.</para>
    /// 
    /// <example>
    ///     <code>
    ///         C:\> Get-Device dc-1 | Get-NotificationTrigger | Remove-NotificationTrigger -WhatIf
    ///         "What if: Performing operation "Remove-NotificationTrigger" on target "'Email to Administrator' (Object ID: 2002, Sub ID: 1)""
    ///     </code>
    ///     <para>Preview what will happen when you attempt all triggers from all devices with the name 'dc-1'</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Device dc-1 | Get-NotificationTrigger | Remove-NotificationTrigger -Force</code>
    ///     <para>Remove all notification triggers from devices named 'dc-1' without prompting for confirmation.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Notification-Triggers#remove-1">Online version:</para>
    /// <para type="link">Get-NotificationTrigger</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "NotificationTrigger", SupportsShouldProcess = true)]
    public class RemoveNotificationTrigger : PrtgOperationCmdlet
    {
        /// <summary>
        /// <para type="description">Notification trigger to remove.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, HelpMessage = "The notification trigger to remove.")]
        public NotificationTrigger Trigger { get; set; }

        /// <summary>
        /// <para type="description">Forces a notification trigger to be removed without displaying a confirmation prompt.</para>
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Remove the notification trigger without prompting for confirmation.")]
        public SwitchParameter Force { get; set; }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (ShouldProcess($"'{Trigger.OnNotificationAction}' (Object ID: {Trigger.ObjectId}, Sub ID: {Trigger.SubId})"))
            {
                if (Force.IsPresent || ShouldContinue($"Are you sure you want to delete notification trigger '{Trigger.OnNotificationAction}' (Object ID: {Trigger.ObjectId}, Sub ID: {Trigger.SubId})", "WARNING!"))
                    ExecuteOperation(() => client.RemoveNotificationTrigger(Trigger), $"Removing notification trigger '{Trigger.OnNotificationAction}' from Object {Trigger.ObjectId}");
            }
        }

        internal override string ProgressActivity => "Removing Notification Triggers";
    }
}
