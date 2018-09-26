using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using System.Xml.Serialization;
using PrtgAPI.Helpers;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Base;
using PrtgAPI.Targets;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieves system resources that capable of being monitored or used for monitoring by a new PRTG Sensor.</para>
    /// 
    /// <para type="description">Get-SensorTarget cmdlet identifies resources that are capable of being used as the target of a new PRTG Sensor.
    /// The resources identified by Get-SensorTarget are dynamic in nature. When Get-SensorTarget is executed, the PRTG will interrogate
    /// the PRTG Probe of the specified device in order to identify the resources that are compatible with the target system. For example,
    /// for creating a new WMI Service sensor, PRTG will request the probe enumerate all of the system services that are installed
    /// on the target device.</para>
    /// 
    /// <para type="description">For convenience, Get-SensorTarget allows specifying a -Name used to filter the returned results.
    /// Typically, items returned by Get-SensorTarget should be sent straight to a call to New-SensorParameters, however if you do
    /// not wish to perform any other processing on the results returned by this cmdlet you may automatically generate a set of
    /// parameters for the specified sensor type by specifying the -Parameters (alias: -Params) parameter. When creating sensor parameters
    /// with the -Params parameter, the default sensor name of the specified sensor type will be used.</para>
    /// 
    /// <para type="description">Sensor types not supported by PrtgAPI can still be interrogated by Get-SensorTarget via the -RawType parameter.
    /// Raw sensor type names can be found by inspecting the Id column of items output from the Get-SensorType cmdlet. When operating on raw types,
    /// by default Get-SensorTarget will attempt to guess the name of the data table within PRTG the sensor targets are stored in. If PRTG detects
    /// more than one data table exists, an <see cref="ArgumentException"/> will be thrown listing the names of all of the available tables. The name
    /// of the table to use can then be specified to the -Table parameter.</para>
    /// 
    /// <para type="description">Sensor targets identified for raw types are represented as a "generic" sensor target type. Generic sensor targets
    /// allow accessing both their Name and Value as named properties. Any other properties of the sensor target can be obtained by accessing the
    /// Properties array of the object. Generic sensor targets capable of being used with any <see cref="NewSensorParameters"/> type, including
    /// <see cref="RawSensorParameters"/> and <see cref="DynamicSensorParameters"/>.</para>
    /// 
    /// <para type="description">While resources returned by Get-SensorTarget are guaranteed to be compatible with the target device,
    /// there is no restriction preventing such resources from being used on other devices as well. Attempting to create a sensor
    /// on an incompatible device may succeed, however the sensor will likely enter a <see cref="Status.Down"/> state upon refreshing,
    /// similar to what would happen if the sensor were created normally and then the sensor's target was deleted.</para>
    /// 
    /// <example>
    ///     <code>C:\> $device = Get-Device -Id 1001</code>
    ///     <para>C:\> $services = Get-SensorTarget WmiService *exchange*</para>
    ///     <para>C:\> $params = New-SensorParameters WmiService $services</para>
    ///     <para>C:\> $device | Add-Sensor $params</para>
    ///     <para>Add all WMI Services whose name contains "Exchange" to the Device with ID 1001</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> $device = Get-Device -Id 1001</code>
    ///     <para>C:\> $params = $device | Get-SensorTarget WmiService *exchange* -Params</para>
    ///     <para>C:\> $device | Add-Sensor $params</para>
    ///     <para>Add all WMI Services whose name contains "Exchange" to the Device with ID 1001, creating sensor parameters immediately.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> $targets = Get-Device -Id 1001 | Get-SensorTarget -RawType vmwaredatastoreextern</code>
    ///     <para>Get all VMware Datastores from the device with ID 1001.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> $targets = Get-Device -Id 1001 | Get-SensorTarget -RawType wmivolume</code>
    ///     <para>C:\> $targets | foreach { $_.Properties[3] }</para>
    ///     <para>List the disk type (Local Disk, Compact Disk etc) of all WMI Volume targets.</para>
    /// </example>
    /// 
    /// <para type="link">New-SensorParameters</para>
    /// <para type="link">Add-Sensor</para>
    /// <para type="link">Get-Device</para>
    /// <para type="link">Get-SensorType</para>
    /// 
    /// </summary>
    [OutputType(typeof(SensorTarget<>))]
    [Cmdlet(VerbsCommon.Get, "SensorTarget", DefaultParameterSetName = ParameterSet.Default)]
    public class GetSensorTarget : PrtgProgressCmdlet
    {
        /// <summary>
        /// <para type="description">The device to retrieve sensor targets from. While results returned by Get-SensorTarget are guaranteed to be compatible
        /// with the specified device, returned objects may also be compatible with other devices in PRTG.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public Device Device { get; set; }

        /// <summary>
        /// <para type="description">The type of sensor target to query for. Not all sensor types may require or support target resolution.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSet.Default)]
        public SensorType Type { get; set; }

        /// <summary>
        /// <para type="description">An expression used to filter returned results to those with a specified name.</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 1)]
        public string Name { get; set; }

        /// <summary>
        /// <para type="description">When present, specifies that Get-SensorTarget should automatically wrap the returned items up in a set of sensor
        /// parameters applicable to the specified sensor type.</para>
        /// </summary>
        [Alias("Params")]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.Default)]
        public SwitchParameter Parameters { get; set; }

        /// <summary>
        /// <para type="description">The raw type of sensor target to query for. Types that require additional information before querying (such as Oracle Tablespace) cannot be queried.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Raw)]
        public string RawType { get; set; }

        /// <summary>
        /// <para type="description">The name of the Dropdown List or Checkbox Group the sensor targets belong to. If this value is null,
        /// PrtgAPI will attempt to guess the name of the table.  If this value cannot be guessed or is not valid,
        /// an <see cref="ArgumentException"/> will be thrown listing all possible values.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.Raw)]
        public string Table { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSensorTarget"/> class.
        /// </summary>
        public GetSensorTarget() : base("Sensor Target")
        {
        }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (ParameterSetName == ParameterSet.Default)
            {
                ProcessDefaultParameterSet();
            }
            else
            {
                var type = EnumHelpers.XmlToEnum<XmlEnumAttribute>(RawType, typeof(SensorTypeInternal), false);

                var str = (type as Enum)?.GetDescription() ?? "Sensor Target";

                GetTargets(
                   str,
                   (d, c, t) => client.Targets.GetSensorTargets(d, RawType, Table, c, t),
                   ParametersNotSupported,
                    e => e.Name
                );
            }
        }

        private void ProcessDefaultParameterSet()
        {
            switch (Type)
            {
                case SensorType.ExeXml:
                    GetExeFile();
                    break;
                case SensorType.WmiService:
                    GetWmiService();
                    break;
                case SensorType.SqlServerDB:
                    GetSqlServerQuery();
                    break;
                default:
                    throw new NotImplementedException($"Sensor type '{Type}' is not currently supported");
            }
        }

        private void GetExeFile() =>
            GetTargets(
                "EXE/Script File",
                client.Targets.GetExeXmlFiles,
                e => new ExeXmlSensorParameters(EnsureSingle(e)),
                e => e.Name
            );

        private void GetWmiService() =>
            GetTargets(
                "WMI Service",
                client.Targets.GetWmiServices,
                s => new WmiServiceSensorParameters(s),
                s => s.Name, s => s.DisplayName
            );

        private void GetSqlServerQuery() =>
            GetTargets(
                "SQL Server Query",
                client.Targets.GetSqlServerQueries,
                ParametersNotSupported,
                s => s.Name
            );

        private SensorParametersInternal ParametersNotSupported<T>(List<T> items)
        {
            throw new NotSupportedException($"Creating sensor parameters for sensor type '{Type}' is not supported");
        }

        private T EnsureSingle<T>(List<T> items)
        {
            if (items.Count > 1)
                throw new InvalidOperationException($"Parameters for sensor type {Type} cannot be used against multiple targets in a single request. Please filter objects further with -Name, or create parameters manually with New-SensorParameters");

            if (items.Count == 1)
                return items.First();

            return default(T);
        }

        private void GetTargets<T>(
            string typeDescription,
            Func<int, Func<int, bool>, CancellationToken, List<T>> getItems,
            Func<List<T>, SensorParametersInternal> createParams,
            params Func<T, string>[] nameProperties)
        {
            if (nameProperties.Length == 0)
                throw new NotImplementedException($"Must specify at least one name property resolver for filtering targets of type {Type}");

            TypeDescription = typeDescription;

            WriteProcessProgressRecords(
                f => ParseItems(getItems(Device.Id, f,CancellationToken), createParams, nameProperties)
            );
        }

        private object ParseItems<T>(List<T> items, Func<List<T>, SensorParametersInternal> createParams, Func<T, string>[] nameProperties)
        {
            items = FilterByName(items, nameProperties);

            if (Parameters)
            {
                var parameters = createParams(items);
                parameters.Source = Device;

                return parameters;
            }

            return items;
        }

        private List<T> FilterByName<T>(List<T> items, params Func<T, string>[] nameProperties)
        {
            if (Name != null)
            {
                var wildcard = new WildcardPattern(Name, WildcardOptions.IgnoreCase);

                items = items.Where(i => nameProperties.Any(getProp => wildcard.IsMatch(getProp(i)))).ToList();
            }

            return items;
        }
    }
}
