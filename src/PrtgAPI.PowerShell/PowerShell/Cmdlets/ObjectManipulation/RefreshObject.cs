using System.Management.Automation;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Requests an object and any if its children refresh themselves immediately.</para>
    /// 
    /// <para type="description">The Refresh-Object cmdlet causes an object to refresh itself. Sensor objects automatically
    /// refresh according to their Scanning Interval. Refresh-Object allows you to bypass this interval and request
    /// the sensor update immediately. If Refresh-Object is applied to a Device, Group or Probe, all sensors under
    /// that object will be refreshed.</para>
    /// 
    /// <para type="description">Sensor Factory sensors do not support being manually refreshed.</para>
    /// 
    /// <para type="description">By default, Refresh-Object will operate in Batch Mode. In Batch Mode, Refresh-Object
    /// will not execute a request for each individual object, but will rather store each item in a queue to refresh
    /// all objects at once, via a single request. This allows PrtgAPI to be extremely performant in performing operations
    /// against a large number of objects.</para>
    /// 
    /// <para type="description">If the pipeline is cancelled (either due to a cmdlet throwing an exception
    /// or the user pressing Ctrl-C) before fully completing, Refresh-Object will not generate a request against PRTG.
    /// If you wish to disable Batch Mode and fully process objects individually one at a time, this can be achieved
    /// by specifying -Batch:$false.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-Sensor -Id 2001 | Refresh-Object</code>
    ///     <para>Refresh the sensor with object ID 2001.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Device -Id 2000 | Refresh-Object</code>
    ///     <para>Refresh all sensors under the device with ID 2000.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/State-Manipulation#refresh-1">Online version:</para>
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Get-Device</para>
    /// <para type="link">Get-Group</para>
    /// <para type="link">Get-Probe</para>
    /// </summary>
    [Cmdlet(VerbsData.Update, "Object", SupportsShouldProcess = true)]
    public class RefreshObject : PrtgMultiOperationCmdlet
    {
        /// <summary>
        /// <para type="description">The object to refresh.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, HelpMessage = "The object to refresh.")]
        public SensorOrDeviceOrGroupOrProbe Object { get; set; }

        internal override string ProgressActivity => "Refreshing PRTG Objects";

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (ShouldProcess($"'{Object.Name}' (ID: {Object.Id})"))
                ExecuteOrQueue(Object);
        }

        /// <summary>
        /// Invokes this cmdlet's action against the current object in the pipeline.
        /// </summary>
        protected override void PerformSingleOperation()
        {
            ExecuteOperation(() => client.RefreshObject(Object.Id), $"Refreshing object '{Object.Name}'");
        }

        /// <summary>
        /// Invokes this cmdlet's action against all queued items from the pipeline.
        /// </summary>
        /// <param name="ids">The Object IDs of all queued items.</param>
        protected override void PerformMultiOperation(int[] ids)
        {
            ExecuteMultiOperation(() => client.RefreshObject(ids), $"Refreshing {GetMultiTypeListSummary()}");
        }

        /// <summary>
        /// Returns the current object that should be passed through this cmdlet.
        /// </summary>
        public override object PassThruObject => Object;
    }
}
