using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Adds a new sensor to a PRTG Device.</para>
    /// 
    /// <para type="description">The Add-Sensor cmdlet adds a new sensor to a PRTG Device. When adding a new
    /// sensor, you must first create a <see cref="BaseSensorParameters"/> object that defines the type of
    /// sensor to create and the settings to use in the new object.</para>
    /// 
    /// <para type="description">When adding sensor types that are natively supported by PrtgAPI, Add-Sensor
    /// will validate that all mandatory parameter fields contain values. If a mandatory field is missing
    /// a value, Add-Sensor will throw an InvalidOperationException, listing the field whose value was missing.
    /// When adding unsupported sensor types defined in <see cref="RawSensorParameters"/>, Add-Sensor does not
    /// perform any parameter validation. As such, it is critical to ensure that all parameter names and values
    /// are valid before passing the parameters to the Add-Sensor cmdlet.</para>
    /// 
    /// <example>
    ///     <code>C:\> $params = New-SensorParameters ExeXml "Monitor Traffic" "TrafficMonitor.ps1"</code>
    ///     <para>C:\> Get-Device *fw* | Add-Sensor $params</para>
    ///     <para>Add an EXE/Script Advanced sensor to all firewall devices, using the script "TrafficMonitor.ps1"</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> $raw = @{</code>
    ///     <para>>>     name_ = "my raw sensor"</para>
    ///     <para>>>     tags_ = "xmlexesensor"</para>
    ///     <para>>>     priority_ = 4</para>
    ///     <para>>>     exefile_ = "CustomScript.ps1|CustomScript.ps1||</para>
    ///     <para>>>     exeparams_ = "arg1 arg2 arg3"</para>
    ///     <para>>>     environment_ = 1</para>
    ///     <para>>>     usewindowsauthentication_ = 1</para>
    ///     <para>>>     mutexname_ = "testMutex"</para>
    ///     <para>>>     timeout_ = 70</para>
    ///     <para>>>     writeresult_ = 1</para>
    ///     <para>>>     intervalgroup = 0</para>
    ///     <para>>>     interval_ = "30|30 seconds"</para>
    ///     <para>>>     errorintervalsdown_ = 2</para>
    ///     <para>>>     sensortype = "exexml"</para>
    ///     <para>>> }</para>
    ///     <para>C:\> $params = New-SensorParameters $raw</para>
    ///     <para>C:\> Get-Device dc-1 | Add-Sensor $params</para>
    ///     <para>Add a new EXE/Script Advanced sensor to the device named dc-1 using its raw parameters</para>
    /// </example>
    /// 
    /// <para type="link">Get-Device</para>
    /// <para type="link">New-SensorParameters</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "Sensor", SupportsShouldProcess = true)]
    public class AddSensor : PrtgOperationCmdlet
    {
        /// <summary>
        /// <para type="description">The device the sensor will be created under.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public Device Device { get; set; }

        /// <summary>
        /// <para type="description">A set of parameters whose properties describe the type of sensor to add, to what device, with what settings.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public BaseSensorParameters Parameters { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (ShouldProcess($"{Parameters.Name} (Device ID: {Device.Id}, Type: {Parameters[Parameter.SensorType]}"))
            {
                ExecuteOperation(() => client.AddSensor(Device.Id, Parameters), "Adding PRTG Sensors", $"Adding sensor '{Parameters.Name}' to device ID {Device.Id}");
            }
        }
    }
}
