using System;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Base;
using IDynamicParameters = System.Management.Automation.IDynamicParameters;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Modifies the value of a PRTG Notification Trigger property.</para>
    ///
    /// <para type="description">The Set-NotificationTriggerProperty cmdlet modifies properties and settings of
    /// PRTG Notification Triggers.</para>
    ///
    /// <para type="description">When a value is specified, Set-NotificationTriggerProperty will attempt to parse the value into its expected type.
    /// If the type cannot be parsed, an exception will be thrown indicating the type of the object specified and the type of value that was expected.
    /// In the case of enums, Set-NotificationTriggerProperty will list all valid values of the target type so that you may know how exactly to interface with the
    /// specified property. In the event you wish to modify multiple properties in a single request, Set-NotificationTriggerProperty provides dynamically
    /// generated parameters for each property supported by PrtgAPI.</para>
    ///
    /// <example>
    ///     <code>Get-Sensor -Id 1001 | Get-Trigger | Set-TriggerProperty OnNotificationAction $null</code>
    ///     <para>Remove the OnNotificationAction of all triggers defined on the object with ID 1001.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>Get-Device -Id 2001 | Get-Trigger -Type Threshold | Set-TriggerProperty -Latency 40 -Channel Primary</code>
    ///     <para>Modify the Latency and Channel properties of all Threshold Triggers on the device with ID 2001.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Notification-Triggers#modify-1">Online version:</para>
    /// <para type="link">Add-NotificationTrigger</para>
    /// <para type="link">Get-NotificationTrigger</para>
    /// <para type="link">New-NotificationTriggerParameters</para>
    /// 
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "NotificationTriggerProperty", SupportsShouldProcess = true, DefaultParameterSetName = ParameterSet.Default)]
    public class SetNotificationTriggerProperty : PrtgPassThruCmdlet, IDynamicParameters
    {
        //todo: search for the full/shortened notification trigger names we're updating. also need to update readme

        /// <summary>
        /// <para type="description">Notification Trigger to set the properties of.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Default)]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Dynamic)]
        public NotificationTrigger Trigger
        {
            get { return implementation.Object; }
            set { implementation.Object = value; }
        }

        /// <summary>
        /// <para type="description">ID of the notification trigger's parent object.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Manual)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.DynamicManual)]
        public int ObjectId
        {
            get { return implementation.ObjectId; }
            set { implementation.ObjectId = value; }
        }

        /// <summary>
        /// <para type="description">ID of the trigger to set the properties of.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Manual)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.DynamicManual)]
        public int SubId
        {
            get { return implementation.SubId; }
            set { implementation.SubId = value; }
        }

        /// <summary>
        /// <para type="description">Property of the trigger to set.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSet.Default)]
        [Parameter(Mandatory = true, Position = 2, ParameterSetName = ParameterSet.Manual)]
        public TriggerProperty Property
        {
            get { return implementation.Property; }
            set { implementation.Property = value; }
        }

        /// <summary>
        /// <para type="description">Value to set the property to.</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 1, ParameterSetName = ParameterSet.Default)]
        [Parameter(Mandatory = false, Position = 3, ParameterSetName = ParameterSet.Manual)]
        [AllowEmptyString]
        public object Value
        {
            get { return implementation.Value; }
            set { implementation.Value = value; }
        }

        private InternalSetSubObjectPropertyCmdlet<NotificationTrigger, TriggerParameter, TriggerProperty> implementation;

        private TriggerParameterParser parameterParser;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetNotificationTriggerProperty"/> class.
        /// </summary>
        public SetNotificationTriggerProperty()
        {
            implementation = new InternalSetSubObjectPropertyCmdlet<NotificationTrigger, TriggerParameter, TriggerProperty>(
                this,
                "Object",
                "Notification Trigger",
                CreateTriggerParameter,
                (t, p) => client.SetTriggerProperty(t, p),
                (t, p) => client.SetTriggerProperty(t, p),
                (o, s, p) => client.SetTriggerProperty(o, s, p),
                e => TriggerParameterParser.GetPropertyType((TriggerProperty) e)
            );

            parameterParser = new TriggerParameterParser(client);
        }

        private static Type[] triggerParameterTypes = typeof(TriggerParameters).Assembly.GetTypes().Where(t => typeof(TriggerParameters).IsAssignableFrom(t)).ToArray();

        /// <summary>
        /// Provides an enhanced one-time, preprocessing functionality for the cmdlet.
        /// </summary>
        protected override void BeginProcessingEx()
        {
            implementation.BeginProcessing();

            base.BeginProcessingEx();
        }

        private TriggerParameter CreateTriggerParameter(TriggerProperty property, object value)
        {
            var parameter = new TriggerParameter(property, value);

            parameterParser.UpdateParameterValue(parameter, new Lazy<PrtgObject>(() =>
            {
                if (Trigger != null)
                    return client.GetObject(Trigger.ObjectId);

                return client.GetObject(ObjectId);
            }));

            return parameter;
        }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx() => implementation.ProcessRecord();

        internal override string ProgressActivity => implementation.ProgressActivity;

        /// <summary>
        /// Retrieves an object that defines the dynamic parameters of this cmdlet.
        /// </summary>
        /// <returns>An object that defines the dynamic parameters of this cmdlet.</returns>
        public object GetDynamicParameters() => implementation.GetDynamicParameters();

        /// <summary>
        /// Returns the current object that should be passed through this cmdlet.
        /// </summary>
        public override object PassThruObject => Trigger;
    }
}
