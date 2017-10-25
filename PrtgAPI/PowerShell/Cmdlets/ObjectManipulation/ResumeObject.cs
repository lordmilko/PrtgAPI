using System.Management.Automation;
using PrtgAPI.Objects.Shared;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Resumes an object from a paused or simulated error state.</para>
    /// 
    /// <para type="description">The Resume-Object cmdlet resumes monitoring an object that has previously been interrupted
    /// due to manually pausing or simulating an error state on the object.</para>
    /// 
    /// <example>
    ///     <code>Get-Sensor -Status PausedByUser | Resume-Object</code>
    ///     <para>Resume all sensors that have been paused by the user. Note: if parent object has been manually paused, child objects will appear PausedByUser but will not be able to be unpaused.</para>
    /// </example>
    /// 
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Get-Device</para>
    /// <para type="link">Get-Group</para>
    /// <para type="link">Get-Probe</para>
    /// </summary>
    [Cmdlet(VerbsLifecycle.Resume, "Object", SupportsShouldProcess = true)]
    public class ResumeObject : PrtgOperationCmdlet
    {
        /// <summary>
        /// <para type="description">The object to resume.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public SensorOrDeviceOrGroupOrProbe Object { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (ShouldProcess($"{Object.Name} (ID: {Object.Id})"))
            {
                ExecuteOperation(() => client.ResumeObject(Object.Id), "Resuming PRTG Objects", $"Processing {Object.BaseType.ToString().ToLower()} '{Object}'");
            }
        }
    }
}
