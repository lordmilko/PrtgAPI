using System.Management.Automation;
using PrtgAPI.PowerShell.Base;
using PrtgAPI.Utilities;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Permanently removes an object from PRTG.</para>
    /// 
    /// <para type="description">The Remove-Object cmdlet permanently removes an object from PRTG. Some objects cannot be deleted
    /// (e.g. the root PRTG group (ID 0) and will generate an exception if you attempt to do so.</para>
    /// 
    /// <para type="description">If invoked with no arguments other than the object to be deleted, Remove-Object will prompt for
    /// confirmation of each object to be deleted. If you wish to delete multiple objects, it is recommend to first run
    /// Remove-Object with the -WhatIf parameter, and then re-run with the -Force parameter if the results of -WhatIf look correct.</para>
    /// 
    /// <para type="description">Remove-Object is capable of operating in Batch Mode. In Batch Mode, Remove-Object
    /// will not execute a request for each individual object, but will rather store each item in a queue to remove all objects
    /// at once, via a single request. This allows PrtgAPI to be extremely performant in performing operations
    /// against a large number of objects. Due to the inherent danger in batch removing multiple objects so quickly,
    /// Remove-Object will only operate in Batch mode if -Batch is specified. If Remove-Object is invoked with -Force,
    /// -Batch mode will be automatically enabled unless specified otherwise.</para>
    /// 
    /// <para type="description">When invoked with -WhatIf, Remove-Object will list all objects that would have been deleted,
    /// along with their corresponding object IDs. Even if you are sure of the objects you wish to delete,
    /// it is recommended to always run with -WhatIf first to confirm you have specified the correct objects
    /// and that PrtgAPI has interpreted your request in the way you intended.</para>
    /// 
    /// <example>
    ///     <code>
    ///         C:\> Get-Device dc-1 | Remove-Object -WhatIf
    ///         "What if: Performing the operation "Remove-Object" on target "'dc-1' (ID: 2001)""
    ///     </code>
    ///     <para>Preview what will happen when you attempt to remove all devices named 'dc-1'</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Device dc-1 | Remove-Object -Force</code>
    ///     <para>Remove all devices with name 'dc-1' without prompting for confirmation.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Remove-Object -Id 1001 -Force</code>
    ///     <para>Removes the object with ID 1001 without prompting for confirmation.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Object-Organization#removing-1">Online version:</para>
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Get-Device</para>
    /// <para type="link">Get-Group</para>
    /// <para type="link">Get-Probe</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "Object", SupportsShouldProcess = true, DefaultParameterSetName = ParameterSet.Default)]
    public class RemoveObject : PrtgMultiOperationCmdlet
    {
        /// <summary>
        /// <para type="description">The object to remove.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Default)]
        public PrtgObject Object { get; set; }

        /// <summary>
        /// <para type="description">The ID of the object to remove.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Manual)]
        public int[] Id { get; set; }

        /// <summary>
        /// <para type="description">Forces an object to be removed without displaying a confirmation prompt.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Force { get; set; }

        internal override string ProgressActivity => "Removing PRTG Objects";

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoveObject"/> class.
        /// </summary>
        public RemoveObject()
        {
            batch = null;
        }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (ShouldProcess(GetShouldProcessMessage(Object, Id)))
            {
                if (Force.IsPresent || ShouldContinue(GetShouldContinueMessage(), "WARNING!"))
                {
                    if (Force.IsPresent && batch == null)
                        Batch = true;

                    ExecuteOrQueue(Object);
                }
            }
        }

        private string GetShouldContinueMessage()
        {
            var str = "Are you sure you want to delete ";

            if (Object != null)
                str += $"{Object.GetTypeDescription().ToLower()} '{Object.Name}' (ID: {Object.Id})";
            else
                str += $"object {("ID".Plural(Id))} {(string.Join(", ", Id))}";

            return str;
        }

        /// <summary>
        /// Invokes this cmdlet's action against the current object in the pipeline.
        /// </summary>
        protected override void PerformSingleOperation()
        {
            ExecuteOperation(() => client.RemoveObject(
                GetSingleOperationId(Object, Id)),
                GetSingleOperationProgressMessage(Object, Id, "Removing", TypeDescriptionOrDefault(Object))
            );
        }

        /// <summary>
        /// Invokes this cmdlet's action against the current object in the pipeline.
        /// </summary>
        protected override void PerformMultiOperation(int[] ids)
        {
            ExecuteMultiOperation(() => client.RemoveObject(ids), $"Removing {GetMultiTypeListSummary()}");
        }

        /// <summary>
        /// Returns the current object that should be passed through this cmdlet.
        /// </summary>
        public override object PassThruObject => Object;
    }
}
