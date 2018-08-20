using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrtgAPI.Targets;

namespace PrtgAPI.Request
{
    /// <summary>
    /// Provides methods for retrieving dynamic sensor targets used for creating and modifying sensors.
    /// </summary>
    public class PrtgTargetHelper
    {
        private PrtgClient client;

        internal PrtgTargetHelper(PrtgClient client)
        {
            this.client = client;
        }

        /// <summary>
        /// Retrieves all EXE/Script files that can be used for creating an EXE/Script Advanced sensor on a specified device.
        /// </summary>
        /// <param name="deviceId">The ID of the device to retrieve EXE/Script files for.</param>
        /// <param name="progressCallback">A callback function used to monitor the progress of the request. If this function returns false, the request is aborted and this method returns null.</param>
        /// <returns>If the request is allowed to run to completion, a list of EXE/Script files. If the request is aborted from the progress callback, this method returns null.</returns>
        public List<ExeFileTarget> GetExeXmlFiles(int deviceId, Func<int, bool> progressCallback = null) =>
            client.ResolveSensorTargets(deviceId, SensorType.ExeXml, progressCallback, ExeFileTarget.GetFiles);

        /// <summary>
        /// Asynchronously retrieves all EXE/Script files that can be used for creating an EXE/Script Advanced sensor on a specified device.
        /// </summary>
        /// <param name="deviceId">The ID of the device to retrieve EXE/Script files for.</param>
        /// <param name="progressCallback">A callback function used to monitor the progress of the request. If this function returns false, the request is aborted and this method returns null.</param>
        /// <returns>If the request is allowed to run to completion, a list of EXE/Script files. If the request is aborted from the progress callback, this method returns null.</returns>
        public async Task<List<ExeFileTarget>> GetExeXmlFilesAsync(int deviceId, Func<int, bool> progressCallback = null) =>
            await client.ResolveSensorTargetsAsync(deviceId, SensorType.ExeXml, progressCallback, ExeFileTarget.GetFiles).ConfigureAwait(false);

        /// <summary>
        /// Retrieves all WMI Services that can be used for creating a WMI Service sensor on a specified device.<para/>
        /// If the device does not have any Windows Credentials defined on it (either explicitly or through inheritance) this method will throw a <see cref="PrtgRequestException"/>.
        /// </summary>
        /// <param name="deviceId">The ID of the device to retrieve WMI Services for.</param>
        /// <param name="progressCallback">A callback function used to monitor the progress of the request. If this function returns false, the request is aborted and this method returns null.</param>
        /// <returns>If the request is allowed to run to completion, a list of WMI Services. If the request is aborted from the progress callback, this method returns null.</returns>
        public List<WmiServiceTarget> GetWmiServices(int deviceId, Func<int, bool> progressCallback = null) =>
            client.ResolveSensorTargets(deviceId, SensorType.WmiService, progressCallback, WmiServiceTarget.GetServices);

        /// <summary>
        /// Asynchronously retrieves all WMI Services that can be used for creating a WMI Service sensor on a specified device.<para/>
        /// If the device does not have any Windows Credentials defined on it (either explicitly or through inheritance) this method will throw a <see cref="PrtgRequestException"/>.
        /// </summary>
        /// <param name="deviceId">The ID of the device to retrieve WMI Services for.</param>
        /// <param name="progressCallback">A callback function used to monitor the progress of the request. If this function returns false, the request is aborted and this method returns null.</param>
        /// <returns>If the request is allowed to run to completion, a list of WMI Services. If the request is aborted from the progress callback, this method returns null.</returns>
        public async Task<List<WmiServiceTarget>> GetWmiServicesAsync(int deviceId, Func<int, bool> progressCallback = null) =>
            await client.ResolveSensorTargetsAsync(deviceId, SensorType.WmiService, progressCallback, WmiServiceTarget.GetServices).ConfigureAwait(false);

        /// <summary>
        /// Retrieves all SQL Query files that can be used for querying a Microsoft SQL Server on a specified device.
        /// </summary>
        /// <param name="deviceId">The ID of the device to retrieve SQL Query files for.</param>
        /// <param name="progressCallback">A callback function used to monitor the progress of the request. If this function returns false, the request is aborted and this method returns null.</param>
        /// <returns>If the request is allowed to run to completion, a list of SQL Query files. If the request is aborted from the progress callback, this method returns null.</returns>
        public List<SqlServerQueryTarget> GetSqlServerQueries(int deviceId, Func<int, bool> progressCallback = null) =>
            client.ResolveSensorTargets(deviceId, SensorType.SqlServerDB, progressCallback, SqlServerQueryTarget.GetQueries);

        /// <summary>
        /// Asynchronously retrieves all SQL Query files that can be used for querying a Microsoft SQL Server on a specified device.
        /// </summary>
        /// <param name="deviceId">The ID of the device to retrieve SQL Query files for.</param>
        /// <param name="progressCallback">A callback function used to monitor the progress of the request. If this function returns false, the request is aborted and this method returns null.</param>
        /// <returns>If the request is allowed to run to completion, a list of SQL Query files. If the request is aborted from the progress callback, this method returns null.</returns>
        public async Task<List<SqlServerQueryTarget>> GetSqlServerQueriesAsync(int deviceId, Func<int, bool> progressCallback = null) =>
            await client.ResolveSensorTargetsAsync(deviceId, SensorType.SqlServerDB, progressCallback, SqlServerQueryTarget.GetQueries).ConfigureAwait(false);

        /// <summary>
        /// Retrieves all sensor targets of a sensor type not currently supported by PrtgAPI.
        /// </summary>
        /// <param name="deviceId">The ID of the device to retrieve sensor targets for.</param>
        /// <param name="sensorType">The type of sensor to retrieve sensor targets for.</param>
        /// <param name="tableName">The name of the Dropdown List or Checkbox Group the sensor targets belong to. If this value is null, PrtgAPI will attempt to guess the name of the table. If this value cannot be guessed or is not valid, an <see cref="ArgumentException"/> will be thrown listing all possible values.</param>
        /// <param name="progressCallback">A callback function used to monitor the progress of the request. If this function returns false, the request is aborted and this method returns null.</param>
        /// <returns>If the request is allowed to run to completion, a generic list of sensor targets. If the request is aborted from the progress callback, this method returns null.</returns>
        public List<GenericSensorTarget> GetSensorTargets(int deviceId, string sensorType, string tableName = null, Func<int, bool> progressCallback = null) =>
            client.ResolveSensorTargets(deviceId, sensorType, progressCallback, r => GenericSensorTarget.GetTargets(r, tableName));

        /// <summary>
        /// Asynchronously rtrieves all sensor targets of a sensor type not currently supported by PrtgAPI.
        /// </summary>
        /// <param name="deviceId">The ID of the device to retrieve sensor targets for.</param>
        /// <param name="sensorType">The type of sensor to retrieve sensor targets for.</param>
        /// <param name="tableName">The name of the Dropdown List or Checkbox Group the sensor targets belong to. If this value is null, PrtgAPI will attempt to guess the name of the table. If this value cannot be guessed or is not valid, an <see cref="ArgumentException"/> will be thrown listing all possible values.</param>
        /// <param name="progressCallback">A callback function used to monitor the progress of the request. If this function returns false, the request is aborted and this method returns null.</param>
        /// <returns>If the request is allowed to run to completion, a generic list of sensor targets. If the request is aborted from the progress callback, this method returns null.</returns>
        public async Task<List<GenericSensorTarget>> GetSensorTargetsAsync(int deviceId, string sensorType, string tableName = null, Func<int, bool> progressCallback = null) =>
            await client.ResolveSensorTargetsAsync(deviceId, sensorType, progressCallback, r => GenericSensorTarget.GetTargets(r, tableName)).ConfigureAwait(false);
    }
}
