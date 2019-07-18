using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PrtgAPI.Parameters;
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
        /// <param name="deviceOrId">The device or ID of the device to retrieve EXE/Script files for.</param>
        /// <param name="progressCallback">A callback function used to monitor the progress of the request. If this function returns false, the request is aborted and this method returns null.</param>
        /// <param name="timeout">Duration (in seconds) to wait for sensor targets to resolve.</param>
        /// <param name="token">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="TimeoutException">Sensor targets failed to resolve within the specified timespan.</exception>
        /// <returns>If the request is allowed to run to completion, a list of EXE/Script files. If the request is aborted from the progress callback, this method returns null.</returns>
        public List<ExeFileTarget> GetExeXmlFiles(Either<Device, int> deviceOrId, Func<int, bool> progressCallback = null, int timeout = 60, CancellationToken token = default(CancellationToken)) =>
            client.ResolveSensorTargets(deviceOrId, SensorType.ExeXml, progressCallback, timeout, null, token, ExeFileTarget.GetFiles);

        /// <summary>
        /// Asynchronously retrieves all EXE/Script files that can be used for creating an EXE/Script Advanced sensor on a specified device.
        /// </summary>
        /// <param name="deviceOrId">The device or ID of the device to retrieve EXE/Script files for.</param>
        /// <param name="progressCallback">A callback function used to monitor the progress of the request. If this function returns false, the request is aborted and this method returns null.</param>
        /// <param name="timeout">Duration (in seconds) to wait for sensor targets to resolve.</param>
        /// <param name="token">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="TimeoutException">Sensor targets failed to resolve within the specified timespan.</exception>
        /// <returns>If the request is allowed to run to completion, a list of EXE/Script files. If the request is aborted from the progress callback, this method returns null.</returns>
        public async Task<List<ExeFileTarget>> GetExeXmlFilesAsync(Either<Device, int> deviceOrId, Func<int, bool> progressCallback = null, int timeout = 60, CancellationToken token = default(CancellationToken)) =>
            await client.ResolveSensorTargetsAsync(deviceOrId, SensorType.ExeXml, progressCallback, timeout, null, token, ExeFileTarget.GetFiles).ConfigureAwait(false);

        /// <summary>
        /// Retrieves all WMI Services that can be used for creating a WMI Service sensor on a specified device.<para/>
        /// If the device does not have any Windows Credentials defined on it (either explicitly or through inheritance) this method will throw a <see cref="PrtgRequestException"/>.
        /// </summary>
        /// <param name="deviceOrId">The device or ID of the device to retrieve WMI Services for.</param>
        /// <param name="progressCallback">A callback function used to monitor the progress of the request. If this function returns false, the request is aborted and this method returns null.</param>
        /// <param name="timeout">Duration (in seconds) to wait for sensor targets to resolve.</param>
        /// <param name="token">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="TimeoutException">Sensor targets failed to resolve within the specified timespan.</exception>
        /// <returns>If the request is allowed to run to completion, a list of WMI Services. If the request is aborted from the progress callback, this method returns null.</returns>
        public List<WmiServiceTarget> GetWmiServices(Either<Device, int> deviceOrId, Func<int, bool> progressCallback = null, int timeout = 60, CancellationToken token = default(CancellationToken)) =>
            client.ResolveSensorTargets(deviceOrId, SensorType.WmiService, progressCallback, timeout, null, token, WmiServiceTarget.GetServices);

        /// <summary>
        /// Asynchronously retrieves all WMI Services that can be used for creating a WMI Service sensor on a specified device.<para/>
        /// If the device does not have any Windows Credentials defined on it (either explicitly or through inheritance) this method will throw a <see cref="PrtgRequestException"/>.
        /// </summary>
        /// <param name="deviceOrId">The device or ID of the device to retrieve WMI Services for.</param>
        /// <param name="progressCallback">A callback function used to monitor the progress of the request. If this function returns false, the request is aborted and this method returns null.</param>
        /// <param name="timeout">Duration (in seconds) to wait for sensor targets to resolve.</param>
        /// <param name="token">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="TimeoutException">Sensor targets failed to resolve within the specified timespan.</exception>
        /// <returns>If the request is allowed to run to completion, a list of WMI Services. If the request is aborted from the progress callback, this method returns null.</returns>
        public async Task<List<WmiServiceTarget>> GetWmiServicesAsync(Either<Device, int> deviceOrId, Func<int, bool> progressCallback = null, int timeout = 60, CancellationToken token = default(CancellationToken)) =>
            await client.ResolveSensorTargetsAsync(deviceOrId, SensorType.WmiService, progressCallback, timeout, null, token, WmiServiceTarget.GetServices).ConfigureAwait(false);

        /// <summary>
        /// Retrieves all SQL Query files that can be used for querying a Microsoft SQL Server on a specified device.
        /// </summary>
        /// <param name="deviceOrId">The device or ID of the device to retrieve SQL Query files for.</param>
        /// <param name="progressCallback">A callback function used to monitor the progress of the request. If this function returns false, the request is aborted and this method returns null.</param>
        /// <param name="timeout">Duration (in seconds) to wait for sensor targets to resolve.</param>
        /// <param name="token">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="TimeoutException">Sensor targets failed to resolve within the specified timespan.</exception>
        /// <returns>If the request is allowed to run to completion, a list of SQL Query files. If the request is aborted from the progress callback, this method returns null.</returns>
        public List<SqlServerQueryTarget> GetSqlServerQueries(Either<Device, int> deviceOrId, Func<int, bool> progressCallback = null, int timeout = 60, CancellationToken token = default(CancellationToken)) =>
            client.ResolveSensorTargets(deviceOrId, SensorType.SqlServerDB, progressCallback, timeout, null, token, SqlServerQueryTarget.GetQueries);

        /// <summary>
        /// Asynchronously retrieves all SQL Query files that can be used for querying a Microsoft SQL Server on a specified device.
        /// </summary>
        /// <param name="deviceOrId">The device or ID of the device to retrieve SQL Query files for.</param>
        /// <param name="progressCallback">A callback function used to monitor the progress of the request. If this function returns false, the request is aborted and this method returns null.</param>
        /// <param name="timeout">Duration (in seconds) to wait for sensor targets to resolve.</param>
        /// <param name="token">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="TimeoutException">Sensor targets failed to resolve within the specified timespan.</exception>
        /// <returns>If the request is allowed to run to completion, a list of SQL Query files. If the request is aborted from the progress callback, this method returns null.</returns>
        public async Task<List<SqlServerQueryTarget>> GetSqlServerQueriesAsync(Either<Device, int> deviceOrId, Func<int, bool> progressCallback = null, int timeout = 60, CancellationToken token = default(CancellationToken)) =>
            await client.ResolveSensorTargetsAsync(deviceOrId, SensorType.SqlServerDB, progressCallback, timeout, null, token, SqlServerQueryTarget.GetQueries).ConfigureAwait(false);

        /// <summary>
        /// Retrieves all sensor targets of a sensor type not currently supported by PrtgAPI.
        /// </summary>
        /// <param name="deviceOrId">The device or ID of the device to retrieve sensor targets for.</param>
        /// <param name="sensorType">The type of sensor to retrieve sensor targets for.</param>
        /// <param name="tableName">The name of the Dropdown List or Checkbox Group the sensor targets belong to. If this value is null, PrtgAPI will attempt to guess the name of the table. If this value cannot be guessed or is not valid, an <see cref="ArgumentException"/> will be thrown listing all possible values.</param>
        /// <param name="progressCallback">A callback function used to monitor the progress of the request. If this function returns false, the request is aborted and this method returns null.</param>
        /// <param name="timeout">Duration (in seconds) to wait for sensor targets to resolve.</param>
        /// <param name="queryParameters">A <see cref="SensorQueryTarget"/>, <see cref="SensorQueryTargetParameters"/> or <see cref="SensorMultiQueryTargetParameters"/> value specifying the parameters that must be known prior to interrogating a sensor's dynamic parameters (such as the SNMP Library file to use or IPMI UserName and Password).</param>
        /// <param name="token">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="TimeoutException">Sensor targets failed to resolve within the specified timespan.</exception>
        /// <returns>If the request is allowed to run to completion, a generic list of sensor targets. If the request is aborted from the progress callback, this method returns null.</returns>
        public List<GenericSensorTarget> GetSensorTargets(Either<Device, int> deviceOrId, string sensorType, string tableName = null, Func<int, bool> progressCallback = null, int timeout = 60, ISensorQueryTargetParameters queryParameters = null, CancellationToken token = default(CancellationToken)) =>
            client.ResolveSensorTargets(deviceOrId, sensorType, progressCallback, timeout, queryParameters, token, r => GenericSensorTarget.GetTargets(r, tableName));

        /// <summary>
        /// Asynchronously rtrieves all sensor targets of a sensor type not currently supported by PrtgAPI.
        /// </summary>
        /// <param name="deviceOrId">The device or ID of the device to retrieve sensor targets for.</param>
        /// <param name="sensorType">The type of sensor to retrieve sensor targets for.</param>
        /// <param name="tableName">The name of the Dropdown List or Checkbox Group the sensor targets belong to. If this value is null, PrtgAPI will attempt to guess the name of the table. If this value cannot be guessed or is not valid, an <see cref="ArgumentException"/> will be thrown listing all possible values.</param>
        /// <param name="progressCallback">A callback function used to monitor the progress of the request. If this function returns false, the request is aborted and this method returns null.</param>
        /// <param name="timeout">Duration (in seconds) to wait for sensor targets to resolve.</param>
        /// <param name="queryParameters">A <see cref="SensorQueryTarget"/>, <see cref="SensorQueryTargetParameters"/> or <see cref="SensorMultiQueryTargetParameters"/> value specifying the parameters that must be known prior to interrogating a sensor's dynamic parameters (such as the SNMP Library file to use or IPMI UserName and Password).</param>
        /// <param name="token">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="TimeoutException">Sensor targets failed to resolve within the specified timespan.</exception>
        /// <returns>If the request is allowed to run to completion, a generic list of sensor targets. If the request is aborted from the progress callback, this method returns null.</returns>
        public async Task<List<GenericSensorTarget>> GetSensorTargetsAsync(Either<Device, int> deviceOrId, string sensorType, string tableName = null, Func<int, bool> progressCallback = null, int timeout = 60, ISensorQueryTargetParameters queryParameters = null, CancellationToken token = default(CancellationToken)) =>
            await client.ResolveSensorTargetsAsync(deviceOrId, sensorType, progressCallback, timeout, queryParameters, token, r => GenericSensorTarget.GetTargets(r, tableName)).ConfigureAwait(false);
    }
}
