using System.Linq;
using System.Management.Automation;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieves all auto-discovery templates supported by a PRTG Server.</para>
    /// 
    /// <para type="description">The Get-DeviceTemplate cmdlet retrieves all device templates supported by a specified object, allowing
    /// you to limit the scope of an auto-discovery operation when creating a new device to a specified set of device types. If no
    /// -<see cref="Device"/> is specified, by default Get-DeviceTemplate will retrieve device templates supported by the Core Probe Device
    /// (Object ID: 40). Practically speaking, all devices should support all device templates; as such, there should generally be no need
    /// to specify an object.</para>
    /// 
    /// <para type="description">Results returned by Get-DeviceTemplate can be filtered by specifying one or more expressions to the
    /// -<see cref="Name"/> parameter. Device template results will be filtered to those that contain a specified expression anywhere
    /// in their Name or Value properties.</para>
    ///
    /// <example>
    ///     <code>
    ///         C:\> $templates = Get-DeviceTemplate *wmi*
    /// 
    ///         C:\> Get-Device -Id 1001 | Start-AutoDiscovery $templates
    ///     </code>
    ///     <para>Perform an auto-discovery for WMI sensors on the device with ID 1001.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>
    ///         C:\> $params = New-DeviceParameters dc-1
    ///         C:\> $params.AutoDiscoveryMode = "AutomaticTemplate"
    ///         C:\> $params.DeviceTemplates = Get-DeviceTemplate *wmi*
    ///
    ///         C:\> Get-Probe contoso | Add-Device $params
    ///     </code>
    ///     <para>Create a new device named "dc-1" that performs an auto-discovery for WMI sensors only.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Object-Creation#auto-discovery-1">Online version:</para>
    /// <para type="link">Start-AutoDiscovery</para> 
    /// <para type="link">New-SensorParameters</para>
    /// <para type="link">Add-Device</para>
    /// 
    /// </summary>
    [OutputType(typeof(DeviceTemplate))]
    [Cmdlet(VerbsCommon.Get, "DeviceTemplate")]
    public class GetDeviceTemplate : PrtgProgressCmdlet
    {
        /// <summary>
        /// <para type="description">The object to retrieve device templates from. If no object is specified, PrtgAPI will retrieve all templates supported by the Core Probe Device (ID 40)</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public Device Device { get; set; }

        /// <summary>
        /// <para type="description">Filters results to those whose name contains the specified expression.</para> 
        /// </summary>
        [Parameter(Mandatory = false, Position = 0)]
        public string[] Name { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetDeviceTemplate"/> class.
        /// </summary>
        public GetDeviceTemplate() : base("Device Template")
        {
        }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            WriteObjectWithProgress(() =>
            {
                var templates = client.GetDeviceTemplates(Device?.Id ?? 40);

                if (Name != null && Name.Length > 0)
                {
                    templates = templates.Where(template => Name
                        .Select(name => new WildcardPattern(name, WildcardOptions.IgnoreCase))
                        .Any(filter => filter.IsMatch(template.Name) || filter.IsMatch(template.Value))
                    ).ToList();
                }

                return templates;
            });
        }
    }
}
