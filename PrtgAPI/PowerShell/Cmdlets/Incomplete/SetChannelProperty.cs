using System;
using System.Management.Automation;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Modifies the value of a channel property.</para>
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
                str = Channel != null ? $"'{Channel.Name}' (Sensor ID: {Channel.SensorId})" : $"Channel {ChannelId} (Sensor ID: {SensorId})";

                SensorId = Channel.SensorId;
                ChannelId = Channel.Id;
            }

            if (ShouldProcess(str, $"Set-ChannelProperty {Property} = '{Value}'"))
                client.SetObjectProperty(SensorId.Value, ChannelId.Value, Property, Value);
        }
    }
}
