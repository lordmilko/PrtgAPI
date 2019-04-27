using System.Management.Automation;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Modifies a notification trigger on a PRTG Server.</para>
    /// 
    /// <para type="description">The Set-NotificationTrigger cmdlet updates an existing notification trigger defined on an object in PRTG.
    /// When editing a notification trigger, you must first create a <see cref="TriggerParameters"/> object that defines the settings you
    /// wish to modify. <see cref="TriggerParameters"/> can be created from an existing notification trigger or created from scratch.</para>
    /// <para type="description">For more information on creating <see cref="TriggerParameters"/>, see New-NotificationTriggerParameters</para>
    /// <para type="description">For information on how to quickly edit a single trigger property, see Edit-NotificationTriggerProperty.</para>
    /// 
    /// <example>
    ///     <code>
    ///         C:\> $triggerParams = Get-Probe | Get-Trigger -Type State | New-TriggerParameters
    ///         C:\> $triggerParams.RepeatInterval = 20
    ///         C:\> $triggerParams.OffNotificationAction = $null
    ///
    ///         C:\> $triggerParams | Set-NotificationTrigger
    ///     </code>
    ///     <para>Set the repeat interval of the notification to trigger to every 20 minutes, and remove the off notification action. This example
    /// assumes there is exactly one probe in the system with exactly one state notification trigger defined on it.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Notification-Triggers#modify-1">Online version:</para>
    /// <para type="link">New-NotificationTriggerParameters</para>
    /// <para type="link">Edit-NotificationTriggerProperty</para>
    /// <para type="link">Get-NotificationTrigger</para> 
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "NotificationTrigger")]
    public class SetNotificationTrigger : PrtgOperationCmdlet
    {
        /// <summary>
        /// <para type="description">The parameters to use to modify a <see cref="NotificationTrigger"/>.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0)]
        public TriggerParameters Parameters { get; set; }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            InternalNotificationTriggerCommand.ProcessRecordEx(this, (a, b) => ExecuteOperation(a, b), Parameters);
        }

        internal override string ProgressActivity => "Updating Notification Triggers";
    }
}