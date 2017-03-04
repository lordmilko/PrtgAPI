using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieve all Notification Actions from a PRTG Server.</para>
    /// 
    /// <para type="description">The Get-NotificationAction retrieves notification actions present on a PRTG Server. Notification actions
    /// can be triggered under various circumstances via notification triggers. For more information on notification triggers, see
    /// Get-NotificationTrigger.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-NotificationTrigger</code>
    ///     <para>Get all notification actions.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-NotificationTrigger *pager*</code>
    ///     <para>Get all notification actions whose name contains "pager"</para>
    /// </example>
    /// 
    /// <para type="link">Get-NotificationTrigger</para>
    /// </summary>
    [OutputType(typeof(NotificationAction))]
    [Cmdlet(VerbsCommon.Get, "NotificationAction")]
    public class GetNotificationAction : PrtgObjectCmdlet<NotificationAction>
    {
        /// <summary>
        /// <para type="description">Filter the response to objects with a certain name. Can include wildcards.</para>
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
