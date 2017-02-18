using System.Management.Automation;
using PrtgAPI.Objects.Shared;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// Resume an object from a paused or simulated error state.
    /// </summary>
    [Cmdlet("Resume", "Object", SupportsShouldProcess = true)]
    public class ResumeObject : PrtgCmdlet
    {
        /// <summary>
        /// The object to resume.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public SensorOrDeviceOrGroupOrProbe Object { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if(ShouldProcess($"{Object.Name} (ID: {Object.Id})"))
                client.Resume(Object.Id);
        }
    }
}
