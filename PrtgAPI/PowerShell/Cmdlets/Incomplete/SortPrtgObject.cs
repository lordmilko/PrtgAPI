using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.Objects.Shared;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets.Incomplete
{
    /// <summary>
    /// <para type="synopsis">Sort the children of a device, group or probe alphabetically.</para>
    /// </summary>
    [Cmdlet(VerbsLifecycle.Start, "SortPrtgObject")]
    public class SortPrtgObject : PrtgCmdlet
    {
        /// <summary>
        /// <para type="description">The device, group or probe whose children should be sorted.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public DeviceOrGroupOrProbe Object { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            client.SortAlphabetically(Object.Id);
        }
    }
}
