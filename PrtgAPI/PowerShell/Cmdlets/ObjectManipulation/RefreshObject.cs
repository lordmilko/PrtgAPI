using System.Management.Automation;
using PrtgAPI.Objects.Shared;
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
    /// <para>Sensor Factory sensors do not support being manually refreshed.</para>
    /// 
    /// <example>
    ///     <code>Get-Sensor -Id 2001 | Refresh-Object</code>
    ///     <para>Refresh the sensor with object ID 2001.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>Get-Device -Id 2000 | Refresh-Object</code>
    ///     <para>Refresh all sensors under the device with ID 2000.</para>
    /// </example>
    /// 
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Get-Device</para>
    /// <para type="link">Get-Group</para>
    /// <para type="link">Get-Probe</para>
    /// </summary>
    [Cmdlet(VerbsData.Update, "Object", SupportsShouldProcess = true)]
    public class RefreshObject : PrtgOperationCmdlet
    {
        /// <summary>
        /// <para type="description">The object to refresh.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, HelpMessage = "The object to refresh.")]
        public SensorOrDeviceOrGroupOrProbe Object { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if(ShouldProcess($"'{Object.Name}' (ID: {Object.Id})"))
                ExecuteOperation(() => client.RefreshObject(Object.Id), "Refreshing PRTG Objects", $"Refreshing object '{Object.Name}'");
        }
    }
}
