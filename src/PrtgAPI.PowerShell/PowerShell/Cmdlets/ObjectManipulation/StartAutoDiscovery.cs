using System;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.PowerShell.Base;
using PrtgAPI.Utilities;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Auto-discovers sensors on a PRTG Device.</para>
    /// 
    /// <para type="description">The Start-AutoDiscovery cmdlet initiates an auto-discovery task against a PRTG Device.
    /// When auto-discovery runs, PRTG will attempt to automatically create sensors under the device based on a series of
    /// built-in templates. If a device is not receptive to a particular sensor type, the sensor is not created.</para>
    /// 
    /// <para type="description">By default, all device templates known to PRTG will be used for performing the auto-discovery.
    /// Templates can be limited those that match one more specified expressions by specifying the -<see cref="TemplateName"/> parameter.
    /// If one or more device templates have already been retrieved via a previous call to Get-DeviceTemplate, these can alternatively
    /// be passed to the -<see cref="Template"/> parameter</para>
    /// 
    /// <para type="description">Sensors have a built-in flag to indicate whether they were created by auto-discovery. If auto-discovery
    /// identifies a sensor type that is applicable to your device that already has a copy of that sensor that was created manually,
    /// auto-discovery will ignore your existing sensor and create a new one alongside it. Because of this, it is always recommended
    /// to use auto-discovery based sensors to allow for running auto-discovery multiple times without causing duplicates.</para>
    /// 
    /// <para type="description">If more than 10 auto-discovery tasks are specified, PRTG will queue the additional tasks
    /// to limit the load on the system.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-Device | Start-AutoDiscovery</code>
    ///     <para>Run auto-discovery against all devices.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Device dc-1 | Start-AutoDiscovery *wmi*</code>
    ///     <para>Run auto-discovery against all devices named "dc-1" using WMI specific device templates</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>
    ///         C:\> $templates = Get-DeviceTemplate *wmi*
    ///
    ///         C:\> Get-Device dc-1 | Start-AutoDiscovery $templates
    ///     </code>
    ///     <para>Run auto-discovery against all devices named "dc-1" using WMI specific device templates</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Object-Creation#auto-discovery-1">Online version:</para>
    /// <para type="link">Get-Device</para>
    /// <para type="link">Get-DeviceTemplate</para>
    /// </summary>
    [Cmdlet(VerbsLifecycle.Start, "AutoDiscovery", SupportsShouldProcess = true, DefaultParameterSetName = ParameterSet.Default)]
    public class StartAutoDiscovery : PrtgPassThruCmdlet
    {
        /// <summary>
        /// <para type="description">The device to perform auto-discovery upon.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public Device Device { get; set; }

        /// <summary>
        /// <para type="description">One or more expressions used to identify device templates to use for the auto-discovery. If no templates
        /// are specified, only templates enabled on the device will be used.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.Default, Position = 0)]
        public string[] TemplateName { get; set; }

        /// <summary>
        /// <para type="description">One or more device templates to use for the auto-discovery. If no templates
        /// are specified, only templates enabled on the device will be used.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.Target, Position = 0)]
        public DeviceTemplate[] Template { get; set; }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (ShouldProcess($"'{Device.Name}' (ID: {Device.Id})"))
            {
                if (TemplateName != null && TemplateName.Length > 0)
                {
                    var templates = client.GetDeviceTemplates(Device.Id);

                    Template = templates.Where(template => TemplateName
                        .Select(name => new WildcardPattern(name, WildcardOptions.IgnoreCase))
                        .Any(filter => filter.IsMatch(template.Name) || filter.IsMatch(template.Value))
                    ).ToArray();

                    if (Template.Length == 0)
                        throw new ArgumentException($"No device templates could be found that match the specified template names {TemplateName.ToQuotedList()}.");
                }

                ExecuteOperation(() => client.AutoDiscover(Device.Id, Template), $"Starting Auto-Discovery on device '{Device}'");
            }
        }

        internal override string ProgressActivity => "PRTG Auto-Discovery";

        /// <summary>
        /// Returns the current object that should be passed through this cmdlet.
        /// </summary>
        public override object PassThruObject => Device;
    }
}
