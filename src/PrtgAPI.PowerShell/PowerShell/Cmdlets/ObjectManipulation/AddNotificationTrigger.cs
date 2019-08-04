using PrtgAPI.Attributes;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Base;
using PrtgAPI.Reflection;
using PrtgAPI.Reflection.Cache;
using PrtgAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using IDynamicParameters = System.Management.Automation.IDynamicParameters;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Adds a notification trigger to a PRTG Server.</para>
    /// 
    /// <para type="description">The Add-NotificationTrigger/New-NotificationTrigger cmdlet adds a new notification  trigger to an object in PRTG.
    /// When adding a notification trigger, you can either specify the settings for the trigger as parameters to the cmdlet (PowerShell style),
    /// or must first create  a <see cref="TriggerParameters"/> object that defines the settings to use (object-oriented style).
    /// <see cref="TriggerParameters"/> can be defined from existing notification triggers or created from scratch.
    /// Certain objects do support certain types of notification triggers (e.g. different types of sensors).</para>
    /// 
    /// <para type="description">Attempting to add a notification trigger to an object that does not supported that trigger
    /// type will generate an <see cref="InvalidTriggerTypeException"/>. Notification triggers applied to parent objects
    /// that are not supported by their children are simply ignored within PRTG, and do not generate exceptions. For
    /// information on viewing the trigger types supported by an object, see Get-NotificationTrigger -Types</para>
    /// 
    /// <para type="description">By default, Add-NotificationTrigger will attempt to resolve the created trigger to a
    /// <see cref="NotificationTrigger"/> object. As PRTG does not return the ID of the created object, PrtgAPI
    /// identifies the newly created trigger by comparing the triggers on the parent object before and after the new trigger is created.
    /// While this is generally very reliable, in the event something or someone else creates another new trigger directly
    /// under the target object with the same OnNotificationAction, that object will also be returned in the objects
    /// resolved by Add-NotificationTrigger. If you do not wish to resolve the created trigger, this behavior can be
    /// disabled by specifying -Resolve:$false.</para>
    /// 
    /// <para type="description">For more information on creating <see cref="TriggerParameters"/> , see New-NotificationTriggerParameters.</para>
    ///
    /// <example>
    ///     <code>C:\ Get-Probe -Id 1001 | New-Trigger -Type State -OnNotificationAction *ticket* -Latency 40</code>
    ///     <para>Create a new state trigger on the probe with ID 1001 that activates the "Ticket Notification" action 40 seconds after being triggered.</para>
    ///     <para/>
    /// </example>
    /// 
    /// <example>
    ///     <code>C:\> Get-Probe | Get-NotificationTrigger | New-TriggerParameters 2001 | Add-Trigger</code>
    ///     <para>Add all triggers on all probes directly to the object with ID 2001.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Notification-Triggers#add-1">Online version:</para>
    /// <para type="link">New-NotificationTriggerParameters</para>
    /// <para type="link">Set-NotificationTrigger</para>
    /// <para type="link">Set-NotificationTriggerProperty</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "NotificationTrigger", SupportsShouldProcess = true, DefaultParameterSetName = ParameterSet.Default)]
    public class AddNotificationTrigger : NewObjectCmdlet, IDynamicParameters
    {
        /// <summary>
        /// <para type="description">The object to create a notification trigger under.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Dynamic)]
        public SensorOrDeviceOrGroupOrProbe Object { get; set; }

        /// <summary>
        /// <para type="description">The type of notification trigger to create.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSet.Dynamic)]
        public TriggerType Type { get; set; }

        /// <summary>
        /// <para type="description">The parameters to use to add a <see cref="NotificationTrigger"/>.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0, ParameterSetName = ParameterSet.Default)]
        public TriggerParameters Parameters { get; set; }

        private PropertyDynamicParameterSet<TriggerProperty> dynamicParams;
        private TriggerParameter[] dynamicParameters;

        private TriggerParameterParser parameterParser;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddNotificationTrigger"/> class.
        /// </summary>
        public AddNotificationTrigger()
        {
            parameterParser = new TriggerParameterParser(client);
        }

        /// <summary>
        /// Provides an enhanced one-time, preprocessing functionality for the cmdlet.
        /// </summary>
        protected override void BeginProcessingEx()
        {
            dynamicParameters = dynamicParams.GetBoundParameters(this, (p, v) => new TriggerParameter(p, PSObjectUtilities.CleanPSObject(v))).ToArray();

            base.BeginProcessingEx();
        }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (ParameterSetName == ParameterSet.Dynamic)
                Parameters = CreateDynamicParameters();

            InternalNotificationTriggerCommand.ProcessRecordEx(
                this,
                (a, b) => ExecuteOperation(a, b),
                Parameters,
                Resolve ? Resolver : (Action)null,
                GetWhatIfAction(),
                GetWhatIfTarget()
            );
        }

        private string GetWhatIfAction()
        {
            if (ParameterSetName == ParameterSet.Dynamic)
            {
                var items = new List<string>();

                foreach (var param in dynamicParameters.OrderBy(p => p.Property != TriggerProperty.OnNotificationAction))
                {
                    var val = param.Value.IsIEnumerable() == true ? string.Join(", ", param.Value.ToIEnumerable()) : param.Value?.ToString();

                    items.Add($"{param.Property} = '{val}'");
                }

                return MyInvocation.MyCommand.Name + ": " + string.Join(", ", items);
            }

            return null;
        }

        private string GetWhatIfTarget()
        {
            if (ParameterSetName == ParameterSet.Dynamic)
            {
                return $"'{Object.Name}' (ID: {Object.Id})";
            }

            return null;
        }

        #region Dynamic

        /// <summary>
        /// Retrieves an object that defines the dynamic parameters of this cmdlet.
        /// </summary>
        /// <returns>An object that defines the dynamic parameters of this cmdlet.</returns>
        public object GetDynamicParameters()
        {
            if (dynamicParams == null)
                dynamicParams = new PropertyDynamicParameterSet<TriggerProperty>(ParameterSet.Dynamic, TriggerParameterParser.GetPropertyType, this);

            return dynamicParams.Parameters;
        }

        private TriggerParameters CreateDynamicParameters()
        {
            UpdateDynamicParameters();

            return TriggerParameters.Create(Type, Object.Id, dynamicParameters);
        }

        internal void UpdateDynamicParameters()
        {
            foreach (var parameter in dynamicParameters)
                parameterParser.UpdateParameterValue(parameter, new Lazy<PrtgObject>(() => Object));
        }

        #endregion

        private void Resolver()
        {
            var objs = AddAndResolveRunner(() => client.AddNotificationTriggerInternal(Parameters, true, CancellationToken, DisplayResolutionError, ShouldStop));

            foreach (var o in objs.OrderBy(i => i.SubId))
                WriteObject(o);
        }

        private List<NotificationTrigger> Except(List<NotificationTrigger> before, List<NotificationTrigger> after)
        {
            return after.Where(a => !before.Any(b => a.ObjectId == b.ObjectId && a.SubId == b.SubId) && a.OnNotificationAction.Id == Parameters.OnNotificationAction.Id).ToList();
        }

        internal override string ProgressActivity => "Adding Notification Triggers";
    }
}
