using System.Management.Automation;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieves the total number of each type of sensor in the system.</para>
    /// 
    /// <para type="description">The Get-SensorTotals cmdlet retrieves the total number of each type of sensor in a PRTG Server.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-SensorTotals</code>
    ///     <para>Count the number of sensors in the system.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Sensors#sensor-totals-1">Online version:</para>
    /// <para type="link">Get-Sensor</para>
    /// </summary>
    [OutputType(typeof(SensorTotals))]
    [Cmdlet(VerbsCommon.Get, "SensorTotals")]
    public class GetSensorTotals : PrtgCmdlet
    {
        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            var result = client.GetSensorTotals();

            WriteObject(result);
        }
    }
}
