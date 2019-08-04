using System.Management.Automation;
using PrtgAPI.Parameters;
using PrtgAPI.Parameters.Helpers;
using PrtgAPI.PowerShell.Base;
using IDynamicParameters = System.Management.Automation.IDynamicParameters;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Modifies the value of a PRTG channel property.</para>
    /// 
    /// <para type="description">The Set-ChannelProperty cmdlet modifies properties and settings of PRTG Channels.
    /// All supported properties that can be modified are typesafe, using the type of the property on the Channel object
    /// returned from Get-Channel.</para>
    /// 
    /// <para type="description">When a value is specified, Set-ChannelProperty will attempt to parse the value into its expected type.
    /// If the type cannot be parsed, an exception will be thrown indicating the type of the object specified and the type that was expected.
    /// In the case of enums, Set-ChannelProperty will list all valid values of the target type so that you may know exactly how to interface
    /// with the specified property. In the event you wish to modify multiple properties in a single request, Set-ChannelProperty provides dynamically
    /// generated parameters for each property supported by PrtgAPI.</para>
    /// 
    /// <para type="description">In the event that a property is specified that has a dependency on another property ebing set (such as UpperErrorLimit
    /// requiring LimitsEnabled be $true) Set-ChannelProperty will automatically assign the required values such that the original property may be
    /// correctly enabled. If the parent of a property is set to the opposite of a child property's required value PrtgAPI will automatically remove
    /// the value specified on the child as well. For example, if LimitsEnabled is set to $false, all error and warning limit related properties
    /// will be set to $null. If LimitsEnabled is later set to $true, you will need to repopulate the values of all error and warning limit properties.</para>
    /// 
    /// <para type="description">For a list of all settings currently supported by Set-ChannelProperty, see Get-Help about_ChannelSettings.</para>
    /// 
    /// <para type="description">By default, Set-ChannelProperty will operate in Batch Mode. In Batch Mode, Set-ChannelProperty
    /// will not execute a request for each individual object, but will rather store each item in a queue to modify channel properties
    /// for all objects at once, via a single request. This allows PrtgAPI to be extremely performant in performing operations
    /// against a large number of objects.</para>
    /// 
    /// <para type="description">If the pipeline is cancelled (either due to a cmdlet throwing an exception
    /// or the user pressing Ctrl-C) before fully completing, Set-ChannelProperty will not generate a request against PRTG.
    /// If you wish to disable Batch Mode and fully process objects individually one at a time, this can be achieved
    /// by specifying -Batch:$false.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-Sensor -Tags wmicpuloadsensor | Get-Channel Total | Set-ChannelProperty UpperErrorLimit 90</code>
    ///     <para>Set the upper error limit of the "Total" channel of all WMI CPU Load sensors to 90. Will also set LimitsEnabled to $true</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>
    ///         C:\> $channels = Get-Sensor -Tags wmicpuloadsensor | Get-Channel Total
    ///
    ///         C:\> $channels | Set-ChannelProperty -UpperErrorLimit 80 -LowerErrorLimit 20
    ///     </code>
    ///     <para>Set the value of both the UpperErrorLimit and LowerErrorLimit in a single request.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Property-Manipulation#modifying-properties-1">Online version:</para>
    /// <para type="link">Get-Help ChannelSettings</para>
    /// <para type="link">Get-Channel</para>
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Set-ObjectProperty</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "ChannelProperty", SupportsShouldProcess = true, DefaultParameterSetName = ParameterSet.Default)]
    public class SetChannelProperty : PrtgMultiOperationCmdlet, IDynamicParameters
    {
        /// <summary>
        /// <para type="description">Channel to set the properties of.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Default)]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Dynamic)]
        public Channel Channel
        {
            get { return implementation.Object; }
            set { implementation.Object = value; }
        }

        /// <summary>
        /// <para type="description">ID of the channel's parent sensor.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Manual)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.DynamicManual)]
        public int SensorId
        {
            get { return implementation.ObjectId; }
            set { implementation.ObjectId = value; }
        }

        /// <summary>
        /// <para type="description">ID of the channel to set the properties of.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Manual)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.DynamicManual)]
        public int ChannelId
        {
            get { return implementation.SubId; }
            set { implementation.SubId = value; }
        }

        /// <summary>
        /// <para type="description">Property of the channel to set.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSet.Default)]
        [Parameter(Mandatory = true, Position = 2, ParameterSetName = ParameterSet.Manual)]
        public ChannelProperty Property
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

        private InternalSetSubObjectPropertyCmdlet<Channel, ChannelParameter, ChannelProperty> implementation;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetChannelProperty"/> class.
        /// </summary>
        public SetChannelProperty()
        {
            implementation = new InternalSetSubObjectPropertyCmdlet<Channel, ChannelParameter, ChannelProperty>(
                this,
                "Sensor",
                "Channel",
                (p, v) => new ChannelParameter(p, v),
                (c, p) => client.SetChannelProperty(c, p),
                (c, p) => client.SetChannelProperty(c, p),
                (o, s, p) => client.SetChannelProperty(o, s, p),
                e => ObjectPropertyParser.GetPropertyInfoViaPropertyParameter<Channel>(e).Property.PropertyType
            );
        }

        /// <summary>
        /// Provides an enhanced one-time, preprocessing functionality for the cmdlet.
        /// </summary>
        protected override void BeginProcessingEx()
        {
            implementation.BeginProcessing();

            base.BeginProcessingEx();
        }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx() => implementation.ProcessRecord();

        internal override string ProgressActivity => implementation.ProgressActivity;

        /// <summary>
        /// Invokes this cmdlet's action against the current object in the pipeline.
        /// </summary>
        protected override void PerformSingleOperation() => implementation.PerformSingleOperation();

        /// <summary>
        /// Invokes this cmdlet's action against the current object in the pipeline.
        /// </summary>
        protected override void PerformMultiOperation(int[] ids) => implementation.PerformMultiOperation(ids);

        /// <summary>
        /// Returns the current object that should be passed through this cmdlet.
        /// </summary>
        public override object PassThruObject => implementation.PassThruObject;

        /// <summary>
        /// Retrieves an object that defines the dynamic parameters of this cmdlet.
        /// </summary>
        /// <returns>An object that defines the dynamic parameters of this cmdlet.</returns>
        public object GetDynamicParameters() => implementation.GetDynamicParameters();
    }
}
