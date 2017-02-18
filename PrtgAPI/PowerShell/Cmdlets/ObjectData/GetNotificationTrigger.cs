using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.Objects.Shared;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// Retrieve notification triggers from a PRTG Server.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "NotificationTrigger")]
    public class GetNotificationTrigger : PrtgObjectCmdlet<NotificationTrigger>
    {
        /// <summary>
        /// The object to retrieve notification triggers for.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, HelpMessage = "The object to retrieve notification triggers for.")]
        public PrtgObject Object { get; set; }

        /// <summary>
        /// Filter the response to objects with a certain OnNotificationAction. Can include wildcards.
        /// </summary>
        [Parameter(Mandatory = false, Position = 0)]
        public string OnNotificationAction { get; set; }

        /// <summary>
        /// Filter the response to objects of a certain type.
        /// </summary>
        [Parameter(Mandatory = false)]
        public TriggerType? Type { get; set; }

        /// <summary>
        /// Indicates whether to include inherited triggers in the response.
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
