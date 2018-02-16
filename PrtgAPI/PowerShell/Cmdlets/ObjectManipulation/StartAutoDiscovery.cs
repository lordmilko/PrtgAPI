using System.Management.Automation;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Auto-discovers sensors on a PRTG Device.</para>
    /// 
    /// <para type="description">The Start-AutoDiscovery cmdlet initiates an auto-discovery task against a PRTG Device.</para>
    /// <para type="description">When auto-discovery runs, PRTG will attempt to automatically create sensors under the device
    /// based on a series of built-in templates. If a device is not receptive to a particular sensor type, the sensor is not created.</para>
    /// <para type="description">Sensors have a built-in flag to indicate whether they were created by auto-discovery. If auto-discovery
    /// identifies a sensor type that is applicable to your device that already has a copy of that sensor that was created manually,
    /// auto-discovery will ignore your existing sensor and create a new one alongside it. Because of this, it is always recommended
    /// to use auto-discovery based sensors to allow for running auto-discovery multiple times without causing duplicates.</para>
    /// <para type="description">If more than 10 auto-discovery tasks are specified, PRTG will queue the additional tasks
    /// to limit the load on the system.</para>
    /// 
    /// <example>
    ///     <code>Get-Device | Start-AutoDiscovery</code>
    ///     <para>Run auto-discovery against all devices.</para>
    /// </example>
    /// 
    /// <para type="link">Get-Device</para>
    /// </summary>
    [Cmdlet(VerbsLifecycle.Start, "AutoDiscovery", SupportsShouldProcess = true)]
    public class StartAutoDiscovery : PrtgPassThruCmdlet
    {
        /// <summary>
        /// <para type="description">The device to perform auto-discovery upon.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public Device Device { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (ShouldProcess($"'{Device.Name}' (ID: {Device.Id})"))
            {
                ExecuteOperation(Device, () => client.AutoDiscover(Device.Id), "PRTG Auto-Discovery", $"Starting Auto-Discovery on device '{Device}'");
            }
        }
    }
}
