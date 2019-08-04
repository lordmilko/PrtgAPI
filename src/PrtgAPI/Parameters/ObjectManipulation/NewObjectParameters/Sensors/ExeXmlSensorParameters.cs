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
        internal override string[] DefaultTags => new[] { "xmlexesensor" };

        /// <summary>
        /// Initializes a new instance of the <see cref="ExeXmlSensorParameters"/> class.
        /// </summary>
        /// <param name="exeFile">The EXE or Script this sensor will execute. Must be located in the Custom Sensors\EXEXML folder on the target device's probe server.</param>
        /// <param name="sensorName">The name to use for the sensor.</param>
        /// <param name="exeParameters">Parameters to pass to the <paramref name="exeFile"/> on each scan.</param>
        /// <param name="setExeEnvironmentVariables">Whether PRTG Environment Variables (%host, %windowsusername, etc) will be available as System Environment Variables inside the EXE/Script.</param>
        /// <param name="useWindowsAuthentication">Whether to use the Windows Credentials of the parent device to execute the specified EXE/Script file. If custom credentials are not used, the file will be executed under the credentials of the PRTG Probe Service.</param>
        /// <param name="mutex">The mutex name to use. All sensors with the same mutex name will be executed sequentially, reducing resource utilization.</param>
        /// <param name="timeout">The duration (in seconds) this sensor can run for before timing out. This value must be between 1-900.</param>
        /// <param name="debugMode">Indicates whether to store raw EXE/Script XML/JSON output for debugging purposes.</param>
        public ExeXmlSensorParameters(ExeFileTarget exeFile, string sensorName = "XML Custom EXE/Script Sensor", string exeParameters = null,
            bool setExeEnvironmentVariables = false, bool useWindowsAuthentication = false,
            string mutex = null, int timeout = 60, DebugMode debugMode = DebugMode.Discard) :
            
            base(sensorName, SensorType.ExeXml)
        {
            if (exeFile == null)
                throw new ArgumentNullException(nameof(exeFile));

            ExeFile = exeFile;

            ExeParameters = exeParameters;
            SetExeEnvironmentVariables = setExeEnvironmentVariables;
            UseWindowsAuthentication = useWindowsAuthentication;
            Mutex = mutex;
            Timeout = timeout;
            DebugMode = debugMode;
        }

        /// <summary>
        /// Gets or sets the EXE or Script file to execute on each scan.
        /// </summary>
        [RequireValue(true)]
        [PropertyParameter(ObjectProperty.ExeFile)]
        public ExeFileTarget ExeFile
        {
            get { return ((ExeFileTarget)GetCustomParameter(ObjectProperty.ExeFile)); }
            set { SetCustomParameter(ObjectProperty.ExeFile, value); }
        }

        /// <summary>
        /// Gets or sets parameters to pass to the <see cref="ExeFile"/> on each scan.
        /// </summary>
        [PropertyParameter(ObjectProperty.ExeParameters)]
        public string ExeParameters
        {
            get { return (string)GetCustomParameter(ObjectProperty.ExeParameters); }
            set { SetCustomParameter(ObjectProperty.ExeParameters, value); }
        }

        /// <summary>
        /// Gets or sets whether PRTG Environment Variables (%host, %windowsusername, etc) will be available as System Environment Variables inside the EXE/Script.
        /// </summary>
        [PropertyParameter(ObjectProperty.SetExeEnvironmentVariables)]
        public bool SetExeEnvironmentVariables
        {
            get { return (bool) GetCustomParameterBool(ObjectProperty.SetExeEnvironmentVariables); }
            set { SetCustomParameterBool(ObjectProperty.SetExeEnvironmentVariables, value); }
        }

        /// <summary>
        /// Gets or sets whether to use the Windows Credentials of the parent device to execute the specified EXE/Script file. If custom credentials are not used, the file will be executed under the credentials of the PRTG Probe Service.
        /// </summary>
        [PropertyParameter(ObjectProperty.UseWindowsAuthentication)]
        public bool UseWindowsAuthentication
        {
            get { return (bool) GetCustomParameterBool(ObjectProperty.UseWindowsAuthentication); }
            set { SetCustomParameterBool(ObjectProperty.UseWindowsAuthentication, value); }
        }

        /// <summary>
        /// Gets or sets the mutex name to use. All sensors with the same mutex name will be executed sequentially, reducing resource utilization.
        /// </summary>
        [PropertyParameter(ObjectProperty.Mutex)]
        public string Mutex
        {
            get { return (string)GetCustomParameter(ObjectProperty.Mutex); }
            set { SetCustomParameter(ObjectProperty.Mutex, value); }
        }

        /// <summary>
        /// Gets or sets the duration (in seconds) this sensor can run for before timing out. This value must be between 1-900.
        /// </summary>
        [PropertyParameter(ObjectProperty.Timeout)]
        public int Timeout
        {
            get { return (int)GetCustomParameter(ObjectProperty.Timeout); }
            set
            {
                if (value > 900)
                    throw new ArgumentException("Timeout must be less than 900.");

                if (value < 1)
                    throw new ArgumentException("Timeout must be greater than or equal to 1.");

                SetCustomParameter(ObjectProperty.Timeout, value);
            }
        }

        /// <summary>
        /// Gets or sets whether to store raw EXE/Script XML/JSON output for debugging purposes.
        /// </summary>
        [PropertyParameter(ObjectProperty.DebugMode)]
        public DebugMode DebugMode
        {
            get { return (DebugMode)GetCustomParameterEnumXml<DebugMode>(ObjectProperty.DebugMode); }
            set { SetCustomParameterEnumXml(ObjectProperty.DebugMode, value); }
        }
    }
}
