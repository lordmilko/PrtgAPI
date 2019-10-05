using System.Linq;
using System.Management.Automation;
using PrtgAPI.PowerShell.Base;
using PrtgAPI.Tree;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Creates a new <see cref="TriggerNode"/> for modelling a PRTG Tree.</para>
    ///
    /// <para type="description">The New-TriggerNode cmdlet creates a new <see cref="TriggerNode"/> object for modelling
    /// a PRTG Tree. Each <see cref="TriggerNode"/> object encapsulates a single <see cref="NotificationTrigger"/> object.</para>
    ///
    /// <para type="description">If an existing <see cref="NotificationTrigger"/> object retrieved from the Get-NotificationTrigger cmdlet
    /// is not specified or piped to the -<see cref="Value"/> parameter, one or more of -<see cref="OnNotificationAction"/>, or -<see cref="SubId"/> or
    /// <see cref="Type"/> parameters must be specified to identify the object to encapsulate. If multiple values are returned as a result
    /// of these parameters, a unique <see cref="TriggerNode"/> object will be created for each item.</para>
    ///
    /// <example>
    ///     <code>C:\> Get-Trigger -ObjectId 1001 | New-TriggerNode</code>
    ///     <para>Create trigger nodes for all notification triggers under the object with ID 1001.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> New-TriggerNode -ObjectId 1001 *ticket* -Type State</code>
    ///     <para>Create trigger nodes for all State notification triggers under the object with ID 1001
    /// that have an OnNotificationAction whose name contains "ticket".</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Tree-Creation">Online version:</para>
    /// <para type="link">Get-NotificationTrigger</para>
    /// <para type="link">New-SensorNode</para>
    /// <para type="link">New-DeviceNode</para>
    /// <para type="link">New-GroupNode</para>
    /// <para type="link">New-ProbeNode</para>
    /// <para type="link">New-PropertyNode</para>
    /// </summary>
    [Cmdlet(VerbsCommon.New, "TriggerNode", DefaultParameterSetName = ParameterSet.Default)]
    public class NewTriggerNode : PrtgNodeCmdlet<TriggerNode, NotificationTrigger>
    {
        /// <summary>
        /// <para type="description">The value to use for the node.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSet.Default, ValueFromPipeline = true)]
        public NotificationTrigger Value { get; set; }

        /// <summary>
        /// <para type="description">The OnNotificationAction to filter by. If this value is a string, can include wildcards.</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 0, ParameterSetName = ParameterSet.Manual)]
        public NameOrObject<NotificationAction>[] OnNotificationAction { get; set; }

        /// <summary>
        /// <para type="description">The ID of the object containing the notification triggers to encapsulate.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Manual)]
        public int ObjectId { get; set; }

        /// <summary>
        /// <para type="description">The Sub IDs of the notification triggers to encapsulate.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.Manual)]
        public int[] SubId { get; set; }

        /// <summary>
        /// <para type="description">The type of notification triggers to encapsulate.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.Manual)]
        public TriggerType[] Type { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewTriggerNode"/> class.
        /// </summary>
        public NewTriggerNode() : base((t, c) => PrtgNode.Trigger(t), () => new GetNotificationTrigger())
        {
        }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            switch (ParameterSetName)
            {
                case ParameterSet.Default:
                    ProcessValues(new[] {Value}, null);
                    break;
                case ParameterSet.Manual:
                    var value = ResolveValues();
                    ProcessValues(value, null);
                    break;
                default:
                    throw new UnknownParameterSetException(ParameterSetName);
            }
        }

        /// <summary>
        /// Validates the results of a manual resolution.
        /// </summary>
        /// <param name="values">The values to validate.</param>
        /// <returns>If the validation completed successfully, true. Otherwise, false.</returns>
        protected override bool VerifyManualResults(NotificationTrigger[] values)
        {
            var result = true;

            if (base.VerifyManualResults(values))
            {
                if (HasParameter(nameof(SubId)))
                {
                    var actualIds = values.Select(v => v.SubId);

                    foreach (var subId in SubId)
                    {
                        if (!actualIds.Contains(subId))
                        {
                            result = false;

                            WriteInvalidOperation($"Could not resolve a {IObjectExtensions.GetTypeDescription(typeof(NotificationTrigger))} with SubID {subId}.");
                        }
                    }
                }
            }
            else
                result = false;

            return result;
        }
    }
}
