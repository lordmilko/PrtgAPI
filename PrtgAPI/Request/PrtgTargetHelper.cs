using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        /// <returns>If the request is allowed to run to completion, a list of EXE/Script files. If the request is aborted from the progress callback, this method returns null.</returns>
        public List<WmiServiceTarget> GetWmiServices(int deviceId, Func<int, bool> progressCallback = null) =>
            client.ResolveSensorTargets(deviceId, SensorType.WmiService, progressCallback, WmiServiceTarget.GetServices);

        /// <summary>
        /// Asynchronously retrieves all WMI Services that can be used for creating a WMI Service sensor on a specified device.<para/>
        /// If the device does not have any Windows Credentials defined on it (either explicitly or through inheritance) this method will throw a <see cref="PrtgRequestException"/>.
        /// </summary>
        /// <param name="deviceId">The ID of the device to retrieve WMI Services for.</param>
        /// <param name="progressCallback">A callback function used to monitor the progress of the request. If this function returns false, the request is aborted and this method returns null.</param>
        /// <returns>If the request is allowed to run to completion, a list of EXE/Script files. If the request is aborted from the progress callback, this method returns null.</returns>
        public async Task<List<WmiServiceTarget>> GetWmiServicesAsync(int deviceId, Func<int, bool> progressCallback = null) =>
            await client.ResolveSensorTargetsAsync(deviceId, SensorType.WmiService, progressCallback, WmiServiceTarget.GetServices).ConfigureAwait(false);
    }
}
