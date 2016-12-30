using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
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
        /// The object ID to retrieve notification triggers for.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, HelpMessage = "The object ID to retrieve notification triggers for.")]
        public PrtgObject ObjectId { get; set; }

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
            var triggers = client.GetNotificationTriggers(ObjectId.Id);

            if (Inherited == false)
                return triggers.Where(a => a.Inherited == false);

            return triggers;
        }
    }
}
