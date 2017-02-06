using System;
using System.Management.Automation;
using PrtgAPI.Attributes;
using PrtgAPI.Objects.Shared;
using PrtgAPI.PowerShell.Base;
using PrtgAPI.Helpers;
using System.Linq;
using System.Text;

namespace PrtgAPI.PowerShell.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "ObjectProperty")]
    public class GetObjectProperty : PrtgCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0)]
        public PrtgObject Object { get; set; }

        /// <summary>
        /// Provides a record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessPrtgRecord()
        {
            var settings = client.GetObjectProperties(Object.Id);
            WriteObject(settings);
        }
    }
}
