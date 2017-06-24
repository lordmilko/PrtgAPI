using System;
using System.Management.Automation;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Modify the value of a channel property.</para>
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
        [Parameter(ParameterSetName = "Manual")]
        public int? SensorId { get; set; }

        /// <summary>
        /// <para type="description">ID of the channel to set the properties of.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Manual")]
        public int? ChannelId { get; set; }

        /// <summary>
        /// <para type="description">Property of the channel to set.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Default")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Manual")]
        public ChannelProperty Property { get; set; }

        /// <summary>
        /// <para type="description">Value to set the property to.</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 2, ParameterSetName = "Default")]
        [Parameter(Mandatory = false, Position = 2, ParameterSetName = "Manual")]
        [AllowEmptyString]
        public object Value { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            //i think we should modify setobjectproperty to detect if we're clearing limits or one of the other ones with fields and clear the associated fields as well. we could POTENTIALLY
            //even have an enum on those properties so we can enumerate all the fields we need to clear

            //what if we have a requiresvalueattribute that throws an error if value is null when its not allowed to be

            //i think this logic can go insite setobjectproperty
            
			//we need to talk about the unit conversion issue in the cmdlet's help

            if (Channel != null)
            {
                SensorId = Channel.SensorId;
                ChannelId = Channel.Id;
            }
            else
            {
                if (SensorId == null)
                    throw new Exception("sensorid is mandatory");
                else if (ChannelId == null)
                    throw new Exception("channelid is mandatory");
            }

            var str = Channel != null ? $"'{Channel.Name}' (Sensor ID: {Channel.SensorId})" : $"Channel {ChannelId} (Sensor ID: {SensorId})";

            if(ShouldProcess(str, $"Set-ChannelProperty {Property} = '{Value}'"))
                client.SetObjectProperty(SensorId.Value, ChannelId.Value, Property, Value); //todo: how do we clear a value on a channel?
        }
    }
}
