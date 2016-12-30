using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.Objects.Shared;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// Request an object and any if its children refresh themselves immediately.
    /// </summary>
    [Cmdlet("Refresh", "Object")]
    public class RefreshObject : PrtgCmdlet
    {
        /// <summary>
        /// The object to refresh.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public SensorOrDeviceOrGroupOrProbe Object { get; set; }

        /// <summary>
        /// Provides a record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            client.CheckNow(Object.Id);
        }
    }
}
