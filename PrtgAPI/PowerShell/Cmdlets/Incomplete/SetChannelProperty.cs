using System;
using System.Management.Automation;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Modifies the value of a channel property.</para>
    /// 
    /// <para type="description">The Set-ChannelProperty cmdlet modifies properties and settings of PRTG Channels.
    /// All supported properties that can be modified are typesafe, using the type of the property on the Channel object
    /// returned from Get-Channel.</para>
    /// <para type="description">When a value is specified, Set-ChannelProperty will attempt to parse the value into its expected type.
    /// If the type cannot be parsed, an exception will be thrown indicating the type of the object specified and the type that was expected.
    /// In the case of enums, Set-ChannelProperty will list all valid values of the target type so that you may know exactly how to interface
    /// with the specified property.</para>
    /// <para type="description">In the event that a property is specified that has a dependency on another property ebing set (such as UpperErrorLimit
    /// requiring LimitsEnabled be $true) Set-ChannelProperty will automatically assign the required values such that the original property may be
    /// correctly enabled. If the parent of a property is set to the opposite of a child property's required value PrtgAPI will automatically remove
    /// the value specified on the child as well. For example, if LimitsEnabled is set to $false, all error and warning limit related properties
    /// will be set to $null. If LimitsEnabled is later set to $true, you will need to repopulate the values of all error and warning limit properties.</para>
    /// <para type="description">For a list of all settings currently supported by Set-ChannelProperty, see Get-Help about_ChannelSettings.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-Sensor -Tags wmicpuloadsensor | Get-Channel Total | Set-ChannelProperty UpperErrorLimit 90</code>
    ///     <para>Set the upper error limit of the "Total" channel of all WMI CPU Load sensors to 90. Will also set LimitsEnabled to $true</para>
    /// </example>
    /// 
    /// <para type="link">about_ChannelSettings</para>
    /// <para type="link">Get-Channel</para>
    /// <para type="link">Get-Sensor</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "ChannelProperty", SupportsShouldProcess = true)]
    public class SetChannelProperty : PrtgCmdlet
    {
        /// <summary>
        /// <para type="description">Channel to set the properties of.</para>
        /// </summary>
        [Parameter(ValueFromPipeline = true, ParameterSetName = "Default")]
        public Channel Channel { get; set; }

        /// <summary>
        /// <para type="description">ID of the channel's parent sensor.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Manual")]
        public int? SensorId { get; set; }

        /// <summary>
        /// <para type="description">ID of the channel to set the properties of.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Manual")]
        public int? ChannelId { get; set; }

        /// <summary>
        /// <para type="description">Property of the channel to set.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Default")]
        [Parameter(Mandatory = true, Position = 2, ParameterSetName = "Manual")]
        public ChannelProperty Property { get; set; }

        /// <summary>
        /// <para type="description">Value to set the property to.</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 1, ParameterSetName = "Default")]
        [Parameter(Mandatory = false, Position = 3, ParameterSetName = "Manual")]
        [AllowEmptyString]
        public object Value { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
			//todo we need to talk about the unit conversion issue in the cmdlet's help

            var str = string.Empty;

            if (!MyInvocation.BoundParameters.ContainsKey("Value"))
                throw new ParameterBindingException("Value parameter is mandatory, however a value was not specified. If Value should be empty, specify $null");

            if (ParameterSetName == "Default")
            {
                str = Channel != null ? $"'{Channel.Name}' (Channel ID: {Channel.Id}, Sensor ID: {Channel.SensorId})" : $"Channel {ChannelId} (Sensor ID: {SensorId})";

                SensorId = Channel.SensorId;
                ChannelId = Channel.Id;
            }

            if (ShouldProcess(str, $"Set-ChannelProperty {Property} = '{Value}'"))
                client.SetObjectProperty(SensorId.Value, ChannelId.Value, Property, Value);
        }
    }
}
