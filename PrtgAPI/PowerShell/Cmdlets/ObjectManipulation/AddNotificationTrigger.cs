using System.Management.Automation;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Adds a notification trigger to a PRTG Server.</para>
    /// 
    /// <para type="description">The Add-NotificationTrigger cmdlet adds a new notification  trigger to an object in PRTG. When adding
    /// a notification trigger, you must first create  a <see cref="TriggerParameters"/>  object that defines the settings to use in
    /// the trigger. <see cref="TriggerParameters"/>  can be defined from existing notification triggers or created from scratch.
    /// Certain objects do support certain types of notification triggers (e.g. different types of sensors). Attempting to add a
    /// notification trigger to an object that does not supported that trigger type will generate an <see cref="InvalidTriggerTypeException"/>.
    /// Notification triggers applied to parent objects that are not supported by their children are simply ignored within PRTG,
    /// and do not generate exceptions. For information on viewing the trigger types supported by an object, see
    /// Get-NotificationTriggerTypes</para>
    /// <para type="description">For more information on creating <see cref="TriggerParameters"/> , see New-NotificationTriggerParameters.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-Probe | Get-NotificationTrigger | New-TriggerParameters 2001 | Add-Trigger</code>
    ///     <para>Add all triggers on all probes directly to the object with ID 2001.</para>
    /// </example>
    /// 
    /// <para type="link">New-NotificationTriggerParameters</para>
    /// <para type="link">Set-NotificationTrigger</para>
    /// <para type="link">Edit-NotificationTriggerProperty</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "NotificationTrigger", SupportsShouldProcess = true)]
    public class AddNotificationTrigger : BaseSetNotificationTrigger
    {
    }

    /// <summary>
    /// <para type="synopsis">Add or modify a notification trigger on a PRTG Server.</para>
    /// 
    /// <para type="description">The Set-NotificationTrigger cmdlet updates an existing notification trigger defined on an object in PRTG.
    /// When editing a notification trigger, you must first create a <see cref="TriggerParameters"/> object that defines the settings you
    /// wish to modify. <see cref="TriggerParameters"/> can be created from an existing notification trigger or created from scratch.</para>
    /// <para type="description">For more information on creating <see cref="TriggerParameters"/>, see New-NotificationTriggerParameters</para>
    /// <para type="description">For information on how to quickly edit a single trigger property, see Edit-NotificationTriggerProperty.</para>
    /// 
    /// <example>
    ///     <para>C:\> $triggerParams = Get-Probe | Get-Trigger -Type State | New-TriggerParameters</para>
    ///     <para>C:\> $triggerParams.RepeatInterval = 20</para>
    ///     <para>C:\> $triggerParams.OffNotificationAction = $null</para>
    ///     <para>C:\> $triggerParams | Set-NotificationTrigger</para>
    ///     <code/>
    ///     <para>Set the repeat interval of the notification to trigger to every 20 minutes, and remove the off notification action. This example
    /// assumes there is exactly one probe in the system with exactly one state notification trigger defined on it.</para>
    /// </example>
    /// 
    /// <para type="link">New-NotificationTriggerParameters</para>
    /// <para type="link">Edit-NotificationTriggerProperty</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "NotificationTrigger")]
    public class SetNotificationTrigger : BaseSetNotificationTrigger
    {
    }

    /// <summary>
    /// Shared functionality for use in <see cref="AddNotificationTrigger"/> and <see cref="SetNotificationTrigger"/>.
    /// </summary>
    public abstract class BaseSetNotificationTrigger : PrtgOperationCmdlet
    {
        /// <summary>
        /// <para type="description">The parameters to use to add/modify a <see cref="NotificationTrigger"/>.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0)]
        public TriggerParameters Parameters { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (ShouldProcess($"Object ID: {Parameters.ObjectId} (Type: {Parameters.Type}, Action: {Parameters.OnNotificationAction})"))
            {
                if (this is AddNotificationTrigger)
                    ExecuteOperation(() => client.SetNotificationTrigger(Parameters), "Updating Notification Triggers", $"Updating notification trigger with ID {Parameters.ObjectId} (Sub ID: {Parameters.SubId})");
                else
                    ExecuteOperation(() => client.AddNotificationTrigger(Parameters), "Adding Notification Triggers", $"Adding notification trigger '{Parameters.OnNotificationAction}' to object ID {Parameters.ObjectId}");
            }
        }
    }
}
