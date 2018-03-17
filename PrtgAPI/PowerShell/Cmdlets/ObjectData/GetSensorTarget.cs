using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Base;

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
    /// </example>
    /// 
    /// <para type="link">New-SensorParameters</para>
    /// <para type="link">Add-Sensor</para>
    /// <para type="link">Get-Device</para>
    /// 
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "SensorTarget")]
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
        [Parameter(Mandatory = true, Position = 0)]
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
        [Parameter(Mandatory = false)]
        public SwitchParameter Parameters { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSensorTarget"/> class.
        /// </summary>
        public GetSensorTarget() : base("Resolve Sensor Targets")
        {
        }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
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
                    throw new NotImplementedException($"Sensor type '{Type}' is currently not supported");
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
            Func<int, Func<int, bool>, List<T>> getItems,
            Func<List<T>, SensorParametersInternal> createParams,
            params Func<T, string>[] nameProperties)
        {
            if (nameProperties.Length == 0)
                throw new NotImplementedException($"Must specify at least one name property resolver for filtering targets of type {Type}");

            TypeDescription = typeDescription;

            WriteProcessProgressRecords<T>(
                f => ParseItems(getItems(Device.Id, f), createParams, nameProperties)
            );
        }

        private object ParseItems<T>(List<T> items, Func<List<T>, SensorParametersInternal> createParams, Func<T, string>[] nameProperties)
        {
            items = FilterByName(items, nameProperties);

            if (Parameters)
            {
                var parameters = createParams(items);
                parameters.targetDevice = Device;

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
