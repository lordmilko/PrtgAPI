using System.Collections.Generic;
using System.Management.Automation;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieves the setting/state modification history of a PRTG Object.</para>
    /// 
    /// <para type="description">The Get-ModificationHistory cmdlet retrieves all setting/state modifications of an object.
    /// The Get-ModificationHistory cmdlet corresponds with the "History" tab of objects in the PRTG UI.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-Sensor -Id 1001 | Get-ModificationHistory</code>
    ///     <para>Retrieve all modification events for the sensor with ID 1001</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Historical-Information#modification-history-1">Online version:</para>
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Get-Device</para>
    /// <para type="link">Get-Group</para>
    /// <para type="link">Get-Probe</para>
    /// </summary>
    [OutputType(typeof(ModificationEvent))]
    [Cmdlet(VerbsCommon.Get, "ModificationHistory")]
    public class GetModificationHistory : PrtgObjectCmdlet<ModificationEvent>
    {
        /// <summary>
        /// <para type="description">The object to retrieve historical data for.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Default, ValueFromPipeline = true)]
        public SensorOrDeviceOrGroupOrProbe Object { get; set; }

        /// <summary>
        /// <para type="description">The ID of the object to retrieve historical data for.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Manual)]
        public int Id { get; set; }

        /// <summary>
        /// Retrieves a list of modification events from a PRTG Server.
        /// </summary>
        /// <returns>A list of all modification events for the specified object.</returns>
        protected override IEnumerable<ModificationEvent> GetRecords()
        {
            if (ParameterSetName == ParameterSet.Default)
                Id = Object.Id;

            var results = client.GetModificationHistory(Id);

            return results;
        }
    }
}
