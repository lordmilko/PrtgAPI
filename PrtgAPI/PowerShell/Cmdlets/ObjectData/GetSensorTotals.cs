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
    /// Retrieve the total number of each type of sensor in the system.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "SensorTotals")]
    public class GetSensorTotals : PrtgCmdlet
    {
        /// <summary>
        /// Provides a record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            var result = client.GetSensorTotals();

            WriteObject(result);
        }
    }
}
