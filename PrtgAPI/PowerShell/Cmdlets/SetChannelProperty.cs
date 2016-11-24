using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// Modify the value of a channel property.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "ChannelProperty")]
    public class SetChannelProperty : PrtgCmdlet
    {
        /// <summary>
        /// Channel to set the properties of.
        /// </summary>
        [Parameter(ValueFromPipeline = true, ParameterSetName = "Default")]
        public Channel Channel { get; set; }

        /// <summary>
        /// ID of the channel's parent sensor.
        /// </summary>
        [Parameter(ParameterSetName = "Manual")]
        public int? SensorId { get; set; }

        /// <summary>
        /// ID of the channel to set the properties of.
        /// </summary>
        [Parameter(ParameterSetName = "Manual")]
        public int? ChannelId { get; set; }

        /// <summary>
        /// Property of the channel to set.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Default")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Manual")]
        public ChannelProperty Property { get; set; }

        /// <summary>
        /// Value to set the property to.
        /// </summary>
        [Parameter(Mandatory = true, Position = 2, ParameterSetName = "Default")]
        [Parameter(Mandatory = true, Position = 2, ParameterSetName = "Manual")]
        [AllowEmptyString]
        public string Value { get; set; }

        /// <summary>
        /// Provides a record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            //i think we should modify setobjectproperty to detect if we're clearing limits or one of the other ones with fields and clear the associated fields as well. we could POTENTIALLY
            //even have an enum on those properties so we can enumerate all the fields we need to clear

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

            client.SetObjectProperty(SensorId.Value, ChannelId.Value, Property, Value); //todo: how do we clear a value on a channel?
        }
    }
}
