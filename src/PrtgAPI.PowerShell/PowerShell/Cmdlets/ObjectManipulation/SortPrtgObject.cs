using System.Management.Automation;
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
    ///     <para>Sort all probes under the root PRTG Group.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Sort-PrtgObject -Id 0</code>
    ///     <para>Sorts all probes under the root PRTG Group.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Object-Organization#sorting-1">Online version:</para>
    /// <para type="link">Get-Device</para>
    /// <para type="link">Get-Group</para>
    /// <para type="link">Get-Probe</para>
    /// </summary>
    [Cmdlet(VerbsLifecycle.Invoke, "SortPrtgObject", SupportsShouldProcess = true, DefaultParameterSetName = ParameterSet.Default)]
    public class SortPrtgObject : PrtgPassThruCmdlet
    {
        /// <summary>
        /// <para type="description">The device, group or probe whose children should be sorted.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Default)]
        public DeviceOrGroupOrProbe Object { get; set; }

        /// <summary>
        /// <para type="description">ID of the object whose children should be sorted.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Manual)]
        public int Id { get; set; }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            var ids = new[] {Id};

            if (ShouldProcess(GetShouldProcessMessage(Object, ids)))
            {
                ExecuteOperation(
                    () => client.SortAlphabetically(GetSingleOperationId(Object, ids)[0]),
                    GetSingleOperationProgressMessage(Object, ids, "Sorting children of", "object")
                );
            }
        }

        internal override string ProgressActivity => "Sorting PRTG Objects";

        /// <summary>
        /// Returns the current object that should be passed through this cmdlet.
        /// </summary>
        public override object PassThruObject => Object;
    }
}
