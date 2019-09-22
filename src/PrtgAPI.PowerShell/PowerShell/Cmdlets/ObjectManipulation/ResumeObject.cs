using System.Management.Automation;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Resumes a PRTG object from a paused or simulated error state.</para>
    /// 
    /// <para type="description">The Resume-Object cmdlet resumes monitoring an object that has previously been stopped
    /// due to manually pausing or simulating an error state on the object.</para>
    /// 
    /// <para type="description">By default, Resume-Object will operate in Batch Mode. In Batch Mode, Resume-Object
    /// will not execute a request for each individual object, but will rather store each item in a queue to resume
    /// all objects at once, via a single request. This allows PrtgAPI to be extremely performant in performing operations
    /// against a large number of objects.</para>
    /// 
    /// <para type="description">If the pipeline is cancelled (either due to a cmdlet throwing an exception
    /// or the user pressing Ctrl-C) before fully completing, Resume-Object will not generate a request against PRTG.
    /// If you wish to disable Batch Mode and fully process objects individually one at a time, this can be achieved
    /// by specifying -Batch:$false.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-Sensor -Status PausedByUser | Resume-Object</code>
    ///     <para>Resume all sensors that have been paused by the user. Note: if parent object has been manually paused, child objects will appear PausedByUser but will not be able to be unpaused.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Resume-Object -Id 1001</code>
    ///     <para>Resumes the object with ID 1001.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/State-Manipulation#resume-1">Online version:</para>
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Get-Device</para>
    /// <para type="link">Get-Group</para>
    /// <para type="link">Get-Probe</para>
    /// </summary>
    [Cmdlet(VerbsLifecycle.Resume, "Object", SupportsShouldProcess = true, DefaultParameterSetName = ParameterSet.Default)]
    public class ResumeObject : PrtgMultiOperationCmdlet
    {
        /// <summary>
        /// <para type="description">The object to resume.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Default)]
        public SensorOrDeviceOrGroupOrProbe Object { get; set; }

        /// <summary>
        /// <para type="description">ID of the object to resume.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Manual)]
        public int[] Id { get; set; }

        internal override string ProgressActivity => "Resuming PRTG Objects";

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (ShouldProcess(GetShouldProcessMessage(Object, Id)))
                ExecuteOrQueue(Object);
        }

        /// <summary>
        /// Invokes this cmdlet's action against the current object in the pipeline.
        /// </summary>
        protected override void PerformSingleOperation()
        {
            ExecuteOperation(
                () => client.ResumeObject(GetSingleOperationId(Object, Id)),
                GetSingleOperationProgressMessage(Object, Id, "Resuming", TypeDescriptionOrDefault(Object))
            );
        }

        /// <summary>
        /// Invokes this cmdlet's action against all queued items from the pipeline.
        /// </summary>
        /// <param name="ids">The Object IDs of all queued items.</param>
        protected override void PerformMultiOperation(int[] ids)
        {
            ExecuteMultiOperation(() => client.ResumeObject(ids), $"Resuming {GetMultiTypeListSummary()}");
        }

        /// <summary>
        /// Returns the current object that should be passed through this cmdlet.
        /// </summary>
        public override object PassThruObject => Object;
    }
}
