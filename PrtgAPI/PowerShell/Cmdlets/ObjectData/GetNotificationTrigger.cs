using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.Objects.Shared;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieve notification triggers from a PRTG Server.</para>
    /// 
    /// <para type="description">The Get-NotificationTrigger cmdlet retrieves notification triggers that are defined on a PRTG Object.
    /// Notification triggers define conditions that when met by a sensor or one of its channels, should result in the firing of a notification
    /// action. When notification triggers are defined on a device, group, or probe, the triggers are inherited by all nodes under the object.
    /// Individual objects can choose to block inheritance of notification triggers, preventing those triggers from trickling down.</para>
    /// <para type="description">When looking at notification triggers defined on a single object, Get-NotificationTrigger can be invoked with no arguments.
    /// When looking at notification triggers across multiple objects, it is often useful to filter out notification triggers inherited from a parent object via
    /// the -Inherited parameter.</para>
    /// <para type="description"><see cref="NotificationTrigger"/> objects returned from Get-NotificationTrigger can be passed to Edit-NotificationTriggerProperty
    /// or  New-NotificationTriggerParameter, to allow cloning or editing the trigger's properties.</para>
    /// 
    /// <example>
    ///     <code>Get-Probe | Get-NotificationTrigger</code>
    ///     <para>Get all notification triggers defined on all probes</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>Get-Device | Get-NotificationTrigger *pager* -Inherited $false</code>
    ///     <para>Get all notification triggers from all devices whose OnNotificationAction action name contains "pager"
    /// in the name and are not inherited from any parent objects.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>Get-Probe -Id 2001 | Get-NotificationTrigger -Type State</code>
    ///     <para>Get all State notification triggers from the sensor with ID 2001</para>
    /// </example>
    /// 
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Get-Device</para>
    /// <para type="link">Get-Group</para>
    /// <para type="link">Get-Probe</para>
    /// <para type="link">Add-NotificationTrigger</para>
    /// <para type="link">New-NotificationTriggerParameter</para>
    /// <para type="link">Set-NotificationTrigger</para>
    /// <para type="link">Edit-NotificationTriggerProperty</para>
    /// </summary>
    [OutputType(typeof(NotificationTrigger))]
    [Cmdlet(VerbsCommon.Get, "NotificationTrigger")]
    public class GetNotificationTrigger : PrtgObjectCmdlet<NotificationTrigger>
    {
        /// <summary>
        /// <para type="description">The object to retrieve notification triggers for.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, HelpMessage = "The object to retrieve notification triggers for.")]
        public PrtgObject Object { get; set; }

        /// <summary>
        /// <para type="description">Filter the response to objects with a certain OnNotificationAction. Can include wildcards.</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 0)]
        public string OnNotificationAction { get; set; }

        /// <summary>
        /// <para type="description">Filter the response to objects of a certain type.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public TriggerType? Type { get; set; }

        /// <summary>
        /// <para type="description">Indicates whether to include inherited triggers in the response.</para>
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Indicates whether to include inherited triggers in the response. If this value is not specified, inherited triggers are included.")]
        public bool? Inherited { get; set; }

        /// <summary>
        /// Retrieves all notification triggers from a PRTG Server.
        /// </summary>
        /// <returns>A list of all notification triggers.</returns>
        protected override IEnumerable<NotificationTrigger> GetRecords()
        {
            IEnumerable<NotificationTrigger> triggers = client.GetNotificationTriggers(Object.Id);

            if (Inherited == false)
                triggers = triggers.Where(a => a.Inherited == false);

            triggers = FilterResponseRecords(triggers, OnNotificationAction, t => t.OnNotificationAction.Name);

            if (Type != null)
                triggers = triggers.Where(t => t.Type == Type.Value);

            return triggers;
        }
    }
}
