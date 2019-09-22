using System.Management.Automation;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Simulates an error status on a PRTG Sensor.</para>
    /// 
    /// <para type="description">The Simulate-ErrorStatus cmdlet forces a PRTG Sensor to enter an error (Down) state.
    /// When an object is in a simulated error status, it will display a message indicating it is in a simulated error state.
    /// Any object put into a simulated error status will remain in this state until the object is resumed via the Resume-Object cmdlet
    /// or via the PRTG UI. Even if an object in a simulated error state is paused, it will return to this state when the object is
    /// unpaused.</para>
    /// 
    /// <para type="description">By default, Simulate-ErrorStatus will operate in Batch Mode. In Batch Mode, Simulate-ErrorStatus
    /// will not execute a request for each individual object, but will rather store each item in a queue to simulate errors
    /// for all objects at once, via a single request. This allows PrtgAPI to be extremely performant in performing operations
    /// against a large number of objects.</para>
    /// 
    /// <para type="description">If the pipeline is cancelled (either due to a cmdlet throwing an exception
    /// or the user pressing Ctrl-C) before fully completing, Simulate-ErrorStatus will not generate a request against PRTG.
    /// If you wish to disable Batch Mode and fully process objects individually one at a time, this can be achieved
    /// by specifying -Batch:$false.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-Sensor -Id 1001 | Simulate-ErrorStatus</code>
    ///     <para>Force the sensor with ID 1001 to enter an error state.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Simulate-ErrorStatus -Id 1001</code>
    ///     <para>Force the sensor with ID 1001 to enter an error state.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/State-Manipulation#simulate-error-1">Online version:</para>
    /// <para type="link">Resume-Object</para>
    /// </summary>
    [Cmdlet(VerbsDiagnostic.Test, "ErrorStatus", SupportsShouldProcess = true, DefaultParameterSetName = ParameterSet.Default)]
    public class SimualteErrorStatus : PrtgMultiOperationCmdlet
    {
        /// <summary>
        /// <para type="description">The sensor to simulate an error status on.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Default)]
        public Sensor Sensor { get; set; }

        /// <summary>
        /// <para type="description">The ID of the sensor to simulate an error status on.</para>
        /// </summary>
        [Alias("SensorId")]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Manual)]
        public int[] Id { get; set; }

        internal override string ProgressActivity => "Simulating Sensor Errors";

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (ShouldProcess(GetShouldProcessMessage(Sensor, Id)))
                ExecuteOrQueue(Sensor);
        }

        /// <summary>
        /// Invokes this cmdlet's action against the current object in the pipeline.
        /// </summary>
        protected override void PerformSingleOperation()
        {
            ExecuteOperation(
                () => client.SimulateError(GetSingleOperationId(Sensor, Id)),
                GetSingleOperationProgressMessage(Sensor, Id, "Processing", "sensor")
            );
        }

        /// <summary>
        /// Invokes this cmdlet's action against all queued items from the pipeline.
        /// </summary>
        /// <param name="ids">The Object IDs of all queued items.</param>
        protected override void PerformMultiOperation(int[] ids)
        {
            ExecuteMultiOperation(() => client.SimulateError(ids), $"Simulating errors on {GetCommonObjectBaseType()} {GetListSummary()}");
        }

        /// <summary>
        /// Returns the current object that should be passed through this cmdlet.
        /// </summary>
        public override object PassThruObject => Sensor;
    }
}
