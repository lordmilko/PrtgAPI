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
        /// Filter the response to objects with a certain name. Can include wildcards.
        /// </summary>
        [Parameter(Mandatory = false, Position = 0)]
        public string Name { get; set; }

        /// <summary>
        /// Retrieves all notification actions from a PRTG Server.
        /// </summary>
        /// <returns>A list of all notification actions.</returns>
        protected override IEnumerable<NotificationAction> GetRecords()
        {
            var actions = client.GetNotificationActions();

            if (Name != null)
            {
                var match = new WildcardPattern(Name.ToLower());

                return actions.Where(action => match.IsMatch(action.Name.ToLower()));
            }
            else
                return actions;
        }
    }
}
