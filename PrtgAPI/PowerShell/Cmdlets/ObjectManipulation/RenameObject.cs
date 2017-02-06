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
    /// Rename a PRTG object.
    /// </summary>
    [Cmdlet(VerbsCommon.Rename, "Object", SupportsShouldProcess = true)]
    public class RenameObject : PrtgCmdlet
    {
        /// <summary>
        /// The object to rename.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public SensorOrDeviceOrGroupOrProbe Object { get; set; }

        /// <summary>
        /// The new name to give the object.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string Name { get; set; }

        /// <summary>
        /// Provides a record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessPrtgRecord()
        {
            if(ShouldProcess($"'{Object.Name}' (ID: {Object.Id})"))
                client.Rename(Object.Id, Name);
        }
    }
}
