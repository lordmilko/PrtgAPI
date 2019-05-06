using System.Linq;
using System.Management.Automation;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieves all sensor types supported by a PRTG Server.</para> 
    /// 
    /// <para type="description">The Get-SensorType cmdlet retrieves all sensor types from PRTG that are supported by a specified object, allowing
    /// you to identify the Type Id to be used with other cmdlets (such as Get-SensorTarget and New-SensorParameters). If no -<see cref="Object"/> or -<see cref="Id"/>
    /// is specified, by default Get-SensorType will retrieve sensor types supported by the Core Probe (Object ID: 1). Practically speaking, all objects appear to support
    /// all sensor types; as such, there should generally be no need to specify an object.</para>
    /// 
    /// <para type="description">Results returned by Get-SensorType can be filtered by specifying an expression to the -Name parameter. Sensor type
    /// results will be filtered to those that contain the specified expression anywhere in either the Id, Name or Description.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-SensorType</code>
    ///     <para>List all sensor types supported by PRTG.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-SensorType *exchange*</code>
    ///     <para>List all sensor types that contain "exchange" in the Id, Name or Description.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Device -Id 1001 | Get-SensorType</code>
    ///     <para>List all sensor types supported by the Device with Id 1001</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Sensors#sensor-types-1">Online version:</para>
    /// <para type="link">Get-SensorTarget</para>
    /// <para type="link">New-SensorParameters</para>
    /// <para type="link">Get-Device</para>
    /// 
    /// </summary>
    [OutputType(typeof(SensorTypeDescriptor))]
    [Cmdlet(VerbsCommon.Get, "SensorType", DefaultParameterSetName = ParameterSet.Default)]
    public class GetSensorType : PrtgCmdlet
    {
        /// <summary>
        /// <para type="description">The object to retrieve sensor types from. If no object is specified, PrtgAPI will retrieve all types supported by the Core Probe (ID 1)</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true, ParameterSetName = ParameterSet.Default)]
        public DeviceOrGroupOrProbe Object { get; set; }

        /// <summary>
        /// <para type="description">The ID of the object to retrieve sensor types from.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Manual)]
        public int Id { get; set; }

        /// <summary>
        /// <para type="description">Filters results to those that contain the specified expression in any field.</para> 
        /// </summary>
        [Parameter(Mandatory = false, Position = 0)]
        public string Name { get; set; }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            var id = GetDeviceId();

            var types = client.GetSensorTypes(id);

            if (Name != null)
            {
                var wildcard = new WildcardPattern(Name, WildcardOptions.IgnoreCase);

                types = types.Where(t => wildcard.IsMatch(t.Id) || wildcard.IsMatch(t.Name) || wildcard.IsMatch(t.Description)).ToList();
            }

            foreach (var type in types)
                WriteObject(type);
        }

        private int GetDeviceId()
        {
            switch (ParameterSetName)
            {
                case ParameterSet.Default:
                    return Object?.Id ?? 1;
                case ParameterSet.Manual:
                    return Id;
                default:
                    throw new UnknownParameterSetException(ParameterSetName);
            }
        }
    }
}
