using System;
using PrtgAPI.Attributes;
using PrtgAPI.Request;
using PrtgAPI.Targets;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgRequestMessage"/> for creating a new EXE/Script Advanced sensor.
    /// </summary>
    public class ExeXmlSensorParameters : SensorParametersInternal
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExeXmlSensorParameters"/> class.
        /// </summary>
        /// <param name="exeFile">The EXE or Script this sensor will execute. Must be located in the Custom Sensors\EXEXML folder on the target device's probe server.</param>
        /// <param name="sensorName">The name to use for this sensor.</param>
        /// <param name="exeParameters">Parameters to pass to the <paramref name="exeFile"/> on each scan.</param>
        /// <param name="setExeEnvironmentVariables">Whether PRTG Environment Variables (%host, %windowsusername, etc) will be available as System Environment Variables inside the EXE/Script.</param>
        /// <param name="useWindowsAuthentication">Whether to use the Windows Credentials of the parent device to execute the specified EXE/Script file. If custom credentials are not used, the file will be executed under the credentials of the PRTG Probe Service.</param>
        /// <param name="mutex">The mutex name to use. All sensors with the same mutex name will be executed sequentially, reducing resource utilization.</param>
        /// <param name="timeout">The duration (in seconds) this sensor can run for before timing out. This value must be between 1-900.</param>
        /// <param name="debugMode">Indicates whether to store raw EXE/Script XML/JSON output for debugging purposes.</param>
        /// <param name="inheritInterval">Whether this sensor's scanning interval settings are inherited from its parent.</param>
        /// <param name="interval">The scanning interval of the sensor. Applies only if <paramref name="inheritInterval"/> is false. If you wish to specify a non-standard scanning interval, you may do so by specifying a <see cref="TimeSpan"/> to property <see cref="NewSensorParameters.Interval"/>.</param>
        /// <param name="intervalErrorMode">The number of scanning intervals the sensor will wait before entering a <see cref="Status.Down"/> state when the sensor reports an error.</param>
        /// <param name="tags">Tags that should be applied to this sensor. If this value is null or no tags are specified, default value is "xmlexesensor".</param>
        public ExeXmlSensorParameters(ExeFileTarget exeFile, string sensorName = "XML Custom EXE/Script Sensor", string exeParameters = null,
            bool setExeEnvironmentVariables = false, bool useWindowsAuthentication = false,
            string mutex = null, int timeout = 60, DebugMode debugMode = DebugMode.Discard, bool inheritInterval = true,
            StandardScanningInterval interval = StandardScanningInterval.SixtySeconds,
            IntervalErrorMode intervalErrorMode = PrtgAPI.IntervalErrorMode.OneWarningThenDown, params string[] tags) :
            
            base(sensorName, SensorType.ExeXml)
        {
            if (exeFile == null)
                throw new ArgumentNullException(nameof(exeFile));

            ExeFile = exeFile;

            if (tags == null || tags.Length == 0)
                Tags = new[] { "xmlexesensor" };

            ExeParameters = exeParameters;
            SetExeEnvironmentVariables = setExeEnvironmentVariables;
            UseWindowsAuthentication = useWindowsAuthentication;
            Mutex = mutex;
            Timeout = timeout;
            DebugMode = debugMode;
            InheritInterval = inheritInterval;
            Interval = interval;
            IntervalErrorMode = intervalErrorMode;
        }

        /// <summary>
        /// Gets or sets the EXE or Script file to execute on each scan.
        /// </summary>
        [RequireValue(true)]
        public ExeFileTarget ExeFile
        {
            get { return ((ExeFileTarget)GetCustomParameter(ObjectProperty.ExeFile)); }
            set { SetCustomParameter(ObjectProperty.ExeFile, value); }
        }

        /// <summary>
        /// Gets or sets parameters to pass to the <see cref="ExeFile"/> on each scan.
        /// </summary>
        public string ExeParameters
        {
            get { return (string)GetCustomParameter(ObjectProperty.ExeParameters); }
            set { SetCustomParameter(ObjectProperty.ExeParameters, value); }
        }

        /// <summary>
        /// Gets or sets whether PRTG Environment Variables (%host, %windowsusername, etc) will be available as System Environment Variables inside the EXE/Script.
        /// </summary>
        public bool? SetExeEnvironmentVariables
        {
            get { return GetCustomParameterBool(ObjectProperty.SetExeEnvironmentVariables); }
            set { SetCustomParameterBool(ObjectProperty.SetExeEnvironmentVariables, value); }
        }

        /// <summary>
        /// Gets or sets whether to use the Windows Credentials of the parent device to execute the specified EXE/Script file. If custom credentials are not used, the file will be executed under the credentials of the PRTG Probe Service.
        /// </summary>
        public bool? UseWindowsAuthentication
        {
            get { return GetCustomParameterBool(ObjectProperty.UseWindowsAuthentication); }
            set { SetCustomParameterBool(ObjectProperty.UseWindowsAuthentication, value); }
        }

        /// <summary>
        /// Gets or sets the mutex name to use. All sensors with the same mutex name will be executed sequentially, reducing resource utilization.
        /// </summary>
        public string Mutex
        {
            get { return (string)GetCustomParameter(ObjectProperty.Mutex); }
            set { SetCustomParameter(ObjectProperty.Mutex, value); }
        }

        /// <summary>
        /// Gets or sets the duration (in seconds) this sensor can run for before timing out. This value must be between 1-900.
        /// </summary>
        public int Timeout
        {
            get { return (int)GetCustomParameter(ObjectProperty.Timeout); }
            set
            {
                if (value > 900)
                    throw new ArgumentException("Timeout must be less than 900");

                if (value < 1)
                    throw new ArgumentException("Timeout must be greater than or equal to 1");

                SetCustomParameter(ObjectProperty.Timeout, value);
            }
        }

        /// <summary>
        /// Gets or sets whether to store raw EXE/Script XML/JSON output for debugging purposes.
        /// </summary>
        public DebugMode DebugMode
        {
            get { return (DebugMode)GetCustomParameterEnumXml<DebugMode>(ObjectProperty.DebugMode); }
            set { SetCustomParameterEnumXml(ObjectProperty.DebugMode, value); }
        }
    }
}
