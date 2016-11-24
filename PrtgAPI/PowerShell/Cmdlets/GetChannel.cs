using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// Retrieve all channels of a sensor.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "Channel")]
    public class GetChannel : PrtgObjectCmdlet<Channel>
    {
        /// <summary>
        /// The Sensor to retrieve channels for.
        /// </summary>
        [Parameter(ValueFromPipeline = true)]
        public Sensor Sensor { get; set; }

        /// <summary>
        /// The ID of the Sensor to retrieve channels for.
        /// </summary>
        [Parameter(ValueFromPipeline = true)]
        public int? SensorId { get; set; }

        /// <summary>
        /// Filter the channels retrieved to only those that match a specific name.
        /// </summary>
        [Parameter(Position = 0)]
        public string Name { get; set; }

        /// <summary>
        /// Retrieves a list of channels from a PRTG Server.
        /// </summary>
        /// <returns>A list of channels.</returns>
        protected override IEnumerable<Channel> GetRecords()
        {
            if (Sensor == null && SensorId == null)
                throw new ArgumentException("Please specify either a Sensor or a SensorId");
            
            if (Sensor != null)
            {
                SensorId = Sensor.Id;
            }

            var results = client.GetChannels(SensorId.Value).OrderBy(c => c.Id).ToList();

            if (Name != null)
            {
                var pattern = new WildcardPattern(Name.ToLower());

                results = results.Where(r => pattern.IsMatch(r.Name.ToLower())).ToList();
            }       

            return results;
        }
    }
}
