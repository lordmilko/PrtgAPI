using System.Management.Automation;
using PrtgAPI.Objects.Shared;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Sorts the children of a device, group or probe alphabetically.</para>
    /// </summary>
    /// <example>
    ///     <code>C:\> Get-Group -Id 0 | Sort-PrtgObject</code>
    ///     <para>Sort all probes under the root PRTG Group</para>
    /// </example>
    [Cmdlet(VerbsLifecycle.Start, "SortPrtgObject")]
    public class SortPrtgObject : PrtgOperationCmdlet
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
            ExecuteOperation(() => client.SortAlphabetically(Object.Id), "Sorting PRTG Objects", $"Sorting children of object {Object.Name} (ID: {Object.Id})");
        }
    }
}
