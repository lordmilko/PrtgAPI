using System.Management.Automation;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// Retrieve the total number of each type of sensor in the system.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "SensorTotals")]
    public class GetSensorTotals : PrtgCmdlet
    {
        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            var result = client.GetSensorTotals();

            WriteObject(result);
        }
    }
}
