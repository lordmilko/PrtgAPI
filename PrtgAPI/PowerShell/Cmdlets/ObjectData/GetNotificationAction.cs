using System.Collections.Generic;
using System.Management.Automation;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieves all Notification Actions from a PRTG Server.</para>
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
    public class GetNotificationAction : PrtgTableFilterCmdlet<NotificationAction, NotificationActionParameters>
    {
        private string[] tags;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetNotificationAction"/> class.
        /// </summary>
        public GetNotificationAction() : base(Content.Notifications, null)
        {
        }

        internal override List<NotificationAction> GetObjectsInternal(NotificationActionParameters parameters)
        {
            return client.GetNotificationActionsInternal(parameters);
        }

        /// <summary>
        /// Processes additional parameters specific to the current cmdlet.
        /// </summary>
        protected override void ProcessAdditionalParameters()
        {
            if (Tags != null)
            {
                tags = Tags;
                Tags = null;
            }

            base.ProcessAdditionalParameters();
        }

        /// <summary>
        /// Process any post retrieval filters specific to the current cmdlet.
        /// </summary>
        /// <param name="records">The records to filter.</param>
        /// <returns>The filtered records.</returns>
        protected override IEnumerable<NotificationAction> PostProcessAdditionalFilters(IEnumerable<NotificationAction> records)
        {
            if (tags != null)
                Tags = tags;

            return base.PostProcessAdditionalFilters(records);
        }

        /// <summary>
        /// Creates a new parameter object to be used for retrieving notification actions from a PRTG Server.
        /// </summary>
        /// <returns>The default set of parameters.</returns>
        protected override NotificationActionParameters CreateParameters() => new NotificationActionParameters();
    }
}
