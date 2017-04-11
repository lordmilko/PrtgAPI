using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieve all channels of a sensor.</para>
    /// 
    /// <para type="description">The Get-Channel cmdlet retrieves all channels beloninging to a PRTG Sensor.</para>
    /// <para type="description">Get-Channel uses APIs unsupported by Paessler to retrieve advanced channel settings (limits, thresholds, etc).
    /// For each channel a separate request must be made in order to retrieve advanced channel details, in addition to a separate single request that retrieves general information for all channels. As such, in order to retrieve information
    /// on n channels, n+1 requests must be made. Keep this in mind when piping sensors with a large number of channels, or piping a large number of sensors.</para> 
    /// <para type="description">If a name is specified, Get-Channel filters the results to those that match the name expression.
    /// Due to limitations of the PRTG API, Get-Channel filters its results post-request.</para>
    /// <para type="description">Get-Channel does not include the Downtime channel present on all sensors (Channel ID -4) as this sensor does not contain a value in table view.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-Sensor -Tags wmicpuloadsensor | Get-Channel</code>
    ///     <para>Get all channels from all Windows CPU Load sensors.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Sensor -Tags wmimemorysensor | Get-Channel Avail*</code>
    ///     <para>Get all channels whose names start with "Avail" from all Windows Memory sensors.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Channel -SensorId 2001</code>
    ///     <para>Get all channels from the sensor with ID 2001</para>
    /// </example>
    /// 
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Set-ChannelProperty</para>
    /// </summary>
    [OutputType(typeof(Channel))]
    [Cmdlet(VerbsCommon.Get, "Channel")]
    public class GetChannel : PrtgObjectCmdlet<Channel>
    {
        /// <summary>
        /// <para type="description">The Sensor to retrieve channels for.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "Sensor")]
        public Sensor Sensor { get; set; }

        /// <summary>
        /// <para type="description">The ID of the Sensor to retrieve channels for.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "SensorId")]
        public int? SensorId { get; set; }

        /// <summary>
        /// <para type="description">Filter the channels retrieved to only those that match a specific name.</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 0)]
        public string Name { get; set; }

        /// <summary>
        /// <para type="description">Filter the channels retrieved to only those that match a specific ID.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public int? Id { get; set; }

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

            if (Id != null)
            {
                results = results.Where(r => r.Id == Id.Value).ToList();
            }

            return results;
        }
    }
}
