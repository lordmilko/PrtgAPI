using System.Management.Automation;
using PrtgAPI.Objects.Shared;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Sorts the children of a device, group or probe alphabetically.</para>
    /// 
    /// <para type="description">The Sort-PrtgObject cmdlet sorts the children of a specified object alphabetically. In addition to normal
    /// devices, groups and probes, this cmdlet can also be used against the Root group (ID: 0), allowing you to sort
    /// probes alphabetically as well.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-Group -Id 0 | Sort-PrtgObject</code>
    ///     <para>Sort all probes under the root PRTG Group</para>
    /// </example>
    /// 
    /// <para type="link">Get-Device</para>
    /// <para type="link">Get-Group</para>
    /// <para type="link">Get-Probe</para>
    /// </summary>
    [Cmdlet(VerbsLifecycle.Start, "SortPrtgObject", SupportsShouldProcess = true)]
    public class SortPrtgObject : PrtgPassThruCmdlet
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
            if (ShouldProcess($"'{Object.Name}' (ID: {Object.Id})"))
                ExecuteOperation(Object, () => client.SortAlphabetically(Object.Id), "Sorting PRTG Objects", $"Sorting children of object {Object.Name} (ID: {Object.Id})");
        }
    }
}
