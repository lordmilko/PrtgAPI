using System.Collections.Generic;
using System.Management.Automation;
using System.Threading;
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
    /// <para type="synopsis">Get-NotificationAction supports filtering returned actions by a number of parameters, including by -Name,
    /// -Id, -Tags or a custom SearchFilter. When filtering by tags, the -Tag parameter can be used, performing a logical OR between all
    /// specified operands. For scenarios in which you wish to filter for notification actions containing ALL specified tags, the -Tags
    /// parameter can be used, performing a logical AND between all specified operands.</para>
    /// 
    /// <para type="description">Get-NotificationAction provides two parameter sets for filtering objects by tags. When filtering for
    /// notification actions that contain one of several tags, the -Tag parameter can be used, performing a logical OR between
    /// all specified operands. For scenarios in which you wish to filter for notification actions containing ALL specified tags,
    /// the -Tags parameter can be used, performing a logical AND between all specified operands.</para>
    /// 
    /// <para type="description">If you are currently signed in as a read only user, you may not be able to see all notification
    /// actions or notification action properties.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-NotificationTrigger</code>
    ///     <para>Get all notification actions.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-NotificationTrigger *pager*</code>
    ///     <para>Get all notification actions whose name contains "pager"</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-NotificationTrigger -Id 301,302</code>
    ///     <para>Get the notification actions with IDs 301 and 302.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-NotificationTrigger -Tags PagerDuty</code>
    ///     <para>Get all notification actions tagged with "PagerDuty"</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Notification-Actions#powershell">Online version:</para>
    /// <para type="link">Get-NotificationTrigger</para>
    /// <para type="link">New-TriggerParameters</para>
    /// </summary>
    [OutputType(typeof(NotificationAction))]
    [Cmdlet(VerbsCommon.Get, "NotificationAction", DefaultParameterSetName = ParameterSet.LogicalAndTags)]
    public class GetNotificationAction : PrtgTableTagCmdlet<NotificationAction, NotificationActionParameters>
    {
        //Notification Actions do not support filtering by tags server side. To account for this, we temporarily
        //remove all tag filters, execute the request, restore the filters and then filter client side
        private string[] tag;
        private string[] tags;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetNotificationAction"/> class.
        /// </summary>
        public GetNotificationAction() : base(Content.Notifications, null)
        {
        }

        internal override List<NotificationAction> GetObjectsInternal(NotificationActionParameters parameters)
        {
            return client.GetNotificationActionsInternal(parameters, CancellationToken.None);
        }

        /// <summary>
        /// Processes additional parameters specific to the current cmdlet.
        /// </summary>
        protected override void ProcessAdditionalParameters()
        {
            StoreParams();

            base.ProcessAdditionalParameters();
        }

        /// <summary>
        /// Process any post retrieval filters specific to the current cmdlet.
        /// </summary>
        /// <param name="records">The records to filter.</param>
        /// <returns>The filtered records.</returns>
        protected override IEnumerable<NotificationAction> PostProcessAdditionalFilters(IEnumerable<NotificationAction> records)
        {
            RestoreParams();

            return base.PostProcessAdditionalFilters(records);
        }

        private void StoreParams()
        {
            if (Tag != null)
            {
                tag = Tag;
                Tag = null;
            }
            if (Tags != null)
            {
                tags = Tags;
                Tags = null;
            }
        }

        private void RestoreParams()
        {
            if (tag != null)
            {
                Tag = tag;
                tag = null;
            }
            if (tags != null)
            {
                Tags = tags;
                tags = null;
            }
        }

        /// <summary>
        /// Creates a new parameter object to be used for retrieving notification actions from a PRTG Server.
        /// </summary>
        /// <returns>The default set of parameters.</returns>
        protected override NotificationActionParameters CreateParameters() => new NotificationActionParameters();
    }
}
