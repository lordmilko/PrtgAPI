using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Request.Serialization;
using PrtgAPI.Targets;

namespace PrtgAPI
{
    /// <summary>
    /// Settings that apply to Sensors within PRTG.
    /// </summary>
    public class SensorSettings : TableSettings
    {
        #region Access Rights

        /// <summary>
        /// Whether to inherit Access Rights settings from the parent object.<para/>
        /// Corresponds to Access Rights -> Inherit Access Rights.
        /// </summary>
        [XmlElement("injected_accessgroup")]
        public bool? InheritAccess { get; set; }

        //todo: the actual access rights

        #endregion
        #region Basic Sensor Settings

        /// <summary>
        /// Tags that are inherited from this objects parent.<para/>
        /// Corresponds to Basic Sensor Settings -> Parent Tags
        /// </summary>
        [StandardSplittableString]
        [XmlElement("injected_parenttags")]
        public string[] ParentTags { get; set; }

        #endregion
        #region Debug Options

        /// <summary>
        /// How raw sensor results should be stored for debugging purposes. Corresponds to:<para/>
        ///     Debug Options (WMI) -> Sensor Result<para/>
        ///     Sensor Settings (EXE/XML) -> EXE Result<para/>
        ///     WMI Service Monitor -> Sensor Result
        /// </summary>
        [XmlElement("injected_writeresult")]
        public DebugMode? DebugMode { get; set; }

        #endregion
        #region HTTP Specific

        //Duplicate properties: Timeout

        /// <summary>
        /// The URL to monitor. If a protocol is not specified, HTTP is used.<para/>
        /// Corresponds to HTTP Specific -> URL.
        /// </summary>
        [XmlElement("injected_httpurl")]
        public string Url { get; set; }

        /// <summary>
        /// The HTTP Request Method to use for requesting the <see cref="Url"/>.<para/>
        /// Corresponds to HTTP Specific -> Request Method.
        /// </summary>
        [XmlElement("injected_httpmethod")]
        public HttpRequestMethod? HttpRequestMethod { get; set; }

        /// <summary>
        /// Data to include in POST requests. Applies when <see cref="HttpRequestMethod"/> is <see cref="PrtgAPI.HttpRequestMethod.POST"/>.<para/>
        /// Corresponds to HTTP Specific -> Postdata.
        /// </summary>
        [XmlElement("injected_postdata")]
        public string PostData { get; set; }

        /// <summary>
        /// Whether POST requests should use a custom content type. If false, content type "application/x-www-form-urlencoded" will be used.<para/>
        /// Corresponds to HTTP Specific -> Content Type.
        /// </summary>
        [XmlElement("injected_postcontentoptions")]
        public bool? UseCustomPostContent { get; set; }

        /// <summary>
        /// Custom content type to use for POST requests.<para/>
        /// Corresponds to HTTP Specific -> Custom Content Type.
        /// </summary>
        [XmlElement("injected_postcontenttype")]
        public string PostContentType { get; set; }

        /// <summary>
        /// The Server Name Indication to use for requests.<para/>
        /// Corresponds to HTTP Specific -> Server Name Indication.
        /// </summary>
        [XmlElement("injected_snihost")]
        internal string SNI { get; set; } //todo: i dont think this property works

        /// <summary>
        /// Whether the Server Name Indication is inherited from the parent device, or derived from the specified <see cref="Url"/>.<para/>
        /// Corresponds to HTTP Specific -> SNI Inheritance.
        /// </summary>
        [XmlElement("injected_sni_inheritance")]
        public bool? UseSNIFromUrl { get; set; }

        #endregion
        #region Ping Settings

        /// <summary>
        /// The duration (in seconds) this sensor can run for before timing out. Based on the sensor type, the maximum valid value may be different. Corresponds to:<para/>
        ///     Ping Settings                 -> Timeout (Max 300)<para/>
        ///     WMI Remote Ping Configuration -> Timeout (Max 300)<para/>
        ///     HTTP Specific                 -> Timeout (Max 900)<para/>
        ///     Sensor Settings (EXE/XML)     -> Timeout (Max 900)
        /// </summary>
        [XmlElement("injected_timeout")]
        public int? Timeout { get; set; }

        /// <summary>
        /// The packet size to use for Ping Requests (bytes). The default value is 32 bytes. This value must be between 1-10,000 bytes.<para/>
        /// Corresponds to Ping Settings -> Packet Size.
        /// </summary>
        [XmlElement("injected_size")]
        public int? PingPacketSize { get; set; }

        /// <summary>
        /// Whether to send a single Ping or Multiple Pings. If <see cref="PrtgAPI.PingMode.MultiPing"/> is used, all packets must be most for the sensor to go <see cref="Status.Down"/>.<para/>
        /// Corresponds to Ping Settings -> Ping Method.
        /// </summary>
        [XmlElement("injected_countmethod")]
        public PingMode? PingMode { get; set; }

        /// <summary>
        /// The number of Ping Requests to make, when using <see cref="PrtgAPI.PingMode.MultiPing"/>.<para/>
        /// Corresponds to Ping Settings -> Ping Count.
        /// </summary>
        [XmlElement("injected_count")]
        public int? PingCount { get; set; }

        /// <summary>
        /// The delay between each Ping Request, when using <see cref="PrtgAPI.PingMode.MultiPing"/> (ms).<para/>
        /// Corresponds to Ping Settings -> Ping Delay.
        /// </summary>
        [XmlElement("injected_delay")]
        public int? PingDelay { get; set; }

        /// <summary>
        /// Whether this sensor should auto acknowledge on entering a <see cref="Status.Down"/> state.<para/>
        /// Corresponds to Ping Settings -> Auto Acknowledge.
        /// </summary>
        [XmlElement("injected_autoacknowledge")]
        public bool? AutoAcknowledge { get; set; }

        #endregion
        #region Sensor Display

        [XmlElement("injected_primarychannel")]
        internal string primaryChannelStr { get; set; }

        internal LazyValue<Channel> primaryChannel;

        /// <summary>
        /// The primary channel of the sensor.<para/>
        /// Corresponds to Sensor Display -> Primary Channel
        /// </summary>
        public Channel PrimaryChannel => primaryChannel?.Value;

        /// <summary>
        /// Whether channels should be shown independently in graphs, or stacked on top of each other.<para/>
        /// Corresponds to Sensor Display -> Graph Type.
        /// </summary>
        [XmlElement("injected_stack")]
        public GraphType GraphType { get; set; }

        [XmlElement("injected_stackunit")]
        internal string StackUnit { get; set; }

        #endregion
        #region Sensor Settings (EXE/XML)

        //Duplicate properties: DebugMode, Timeout, NotifyChanged

        [XmlElement("injected_exefile")]
        internal string exeFileStr { get; set; }

        /// <summary>
        /// Name of the EXE or Script File the sensor executes.<para/>
        /// Corresponds to Sensor Settings -> EXE/Script
        /// </summary>
        public ExeFileTarget ExeFile => exeFileStr;

        /// <summary>
        /// Parameters that will be passed to the specified EXE/Script file.<para/>
        /// Corresponds to Sensor Settings -> Parameters.
        /// </summary>
        [XmlElement("injected_exeparams")]
        public string ExeParameters { get; set; }

        /// <summary>
        /// Whether PRTG Environment Variables (%host, %windowsusername, etc) will be available as System Environment Variables inside the EXE/Script.<para/>
        /// Corresponds to Sensor Settings -> Environment
        /// </summary>
        [XmlElement("injected_environment")]
        public bool? SetExeEnvironmentVariables { get; set; }

        /// <summary>
        /// Whether to use the Windows Credentials of the parent device to execute the specified EXE/Script file. If custom credentials are not used, the file will be executed under the credentials of the PRTG Probe Service.<para/>
        /// Corresponds to Sensor Settings -> Security Context.
        /// </summary>
        [XmlElement("injected_usewindowsauthentication")]
        public bool? UseWindowsAuthentication { get; set; }

        /// <summary>
        /// The mutex name to use. All sensors with the same mutex name will be executed sequentially, reducing resource utilization.<para/>
        /// Corresponds to Sensor Settings -> Mutex Name.
        /// </summary>
        [XmlElement("injected_mutexname")]
        public string Mutex { get; set; }

        //[XmlElement("injected_valuetype")]
        //public ExeValueType? ExeValueType { get; set; } //todo: we're not currently retrieving this value property

        //todo: im pretty sure i saw this under exexml settings as well, but i cant seem to find it again

        #endregion
        #region WMI Alternative Query

        /// <summary>
        /// The method used for performing WMI queries.<para/>
        /// Corresponds to WMI Alternative Query -> Alternative Query.
        /// </summary>
        [XmlElement("injected_wmialternative")]
        public WmiMode? WmiMode { get; set; }

        #endregion
        #region WMI Remote Ping Configuration

        //Duplicate properties: Timeout

        /// <summary>
        /// The DNS Name or IP Address to target.<para/>
        /// Corresponds to WMI Remote Ping Configuration -> Target.
        /// </summary>
        [XmlElement("injected_targetaddress")]
        public string Target { get; set; }

        /// <summary>
        /// The packet size to use for Remote Ping Requests (bytes). The default value is 32 bytes. This value must be between 1-10,000 bytes.<para/>
        /// Corresponds to WMI remote Ping Configuration -> Packet Size.
        /// </summary>
        [XmlElement("injected_buffersize")]
        public int? PingRemotePacketSize { get; set; } //note it can be between 1-10,000 bytes

        #endregion
        #region Sensor Factory Specific Settings

        /// <summary>
        /// Channels to display in a sensor factory<para/>
        /// Corresponds to Sensor Factory Specific Settings -> Channel Definition
        /// </summary>
        [SplittableString('\n')]
        [XmlElement("injected_aggregationchannel")]
        public string[] ChannelDefinition { get; set; }

        /// <summary>
        /// How the sensor should respond when one of its source sensors goes <see cref="Status.Down"/><para/>
        /// Corresponds to Sensor Factory Specific Settings -> Error Handling.
        /// </summary>
        [XmlElement("injected_warnonerror")]
        public FactoryErrorMode? FactoryErrorMode { get; set; }

        /// <summary>
        /// Custom formula for determining when a sensor sensor factory should go <see cref="Status.Down"/> when one
        /// of its source sensors enters an error state.<para/>Applies when <see cref="FactoryErrorMode"/> is
        /// <see cref="PrtgAPI.FactoryErrorMode.CustomFormula"/><para/>
        /// Corresponds to Sensor Factory Specific Settings -> Status Definition.
        /// </summary>
        [XmlElement("injected_aggregationstatus")]
        public string FactoryErrorFormula { get; set; }

        /// <summary>
        /// How factory channel values should be calculated when one of their source sensors is <see cref="Status.Down"/><para/>
        /// Corresponds to Sensor Factory Specific Settings -> If a Sensor has No Data
        /// </summary>
        [XmlElement("injected_missingdata")]
        public FactoryMissingDataMode? FactoryMissingDataMode { get; set; }

        #endregion
        #region WMI Service Monitor

        //Duplicate properties: DebugMode

        /// <summary>
        /// Whether PRTG should automatically start this item if it detects it has stopped<para/>
        /// Corresponds to WMI Service Monitor -> If Service is Not Running.
        /// </summary>
        [XmlElement("injected_restart")]
        public bool? StartStopped { get; set; }

        /// <summary>
        /// Whether PRTG should trigger any Change notification triggers defined on this object when PRTG starts the object or its value changes.<para/>
        /// For specific trigger conditions see documentation for object type. Corresponds to<para/>
        ///     Sensor Settings -> If Value Changes
        ///     WMI Service Monitor -> If Service is Restarted.
        /// </summary>
        [XmlElement("injected_monitorchange")]
        public bool? NotifyChanged { get; set; }

        /// <summary>
        /// Whether PRTG should monitor just basic information for the target of the sensor, or should also collect additional performance metrics.<para/>
        /// Corresponds to WMI Service Monitor -> Monitoring.
        /// </summary>
        [XmlElement("injected_monitorextended")]
        public bool? MonitorPerformance { get; set; }

        /// <summary>
        /// The name of the service. Is not shown in the PRTG UI; only the Service Display Name is shown.
        /// </summary>
        [XmlElement("injected_service")]
        public string ServiceName { get; set; }

        #endregion
        #region Database Specific

        /// <summary>
        /// The name of the database this sensor targets.<para/>
        /// Corresponds to Database Specific -> Database.
        /// </summary>
        [XmlElement("injected_database")]
        public string Database { get; set; }

        /// <summary>
        /// Whether to use a custom instance of the SQL Server, or use the server's default instance.<para/>
        /// Corresponds to Data Specific -> SQL Server Instance.
        /// </summary>
        [XmlElement("injected_useinstancename")]
        public bool? UseCustomInstance { get; set; }

        /// <summary>
        /// The name of the custom instance of SQL Server to connect to.<para/>
        /// Corresponds to Database Specific -> Instance Name.
        /// </summary>
        [XmlElement("injected_instancename")]
        public string InstanceName { get; set; }

        /// <summary>
        /// How to encrypt the connection between PRTG and the target database server.<para/>
        /// Corresponds to Database Specific -> Encryption.
        /// </summary>
        [XmlElement("injected_enforceencryption")]
        public SqlEncryptionMode? SqlEncryptionMode { get; set; }

        #endregion
        #region Data

        /// <summary>
        /// Whether to pass any parameters to the specified <see cref="ObjectProperty.SqlServerQuery"/>.<para/>
        /// Corresponds to Data -> Use Input Parameter.
        /// </summary>
        [XmlElement("injected_useparam")]
        public bool? UseSqlInputParameter { get; set; }

        /// <summary>
        /// Parameters to pass to the specified <see cref="ObjectProperty.SqlServerQuery"/>.<para/>
        /// Corresponds to Data -> Input Parameter.
        /// </summary>
        [XmlElement("injected_param")]
        public string SqlInputParameter { get; set; }

        /// <summary>
        /// Whether PRTG should use transactional processing for executing the specified <see cref="ObjectProperty.SqlServerQuery"/>.<para/>
        /// Corresponds to Data -> Use Transaction.
        /// </summary>
        [XmlElement("injected_transaction")]
        public SqlTransactionMode? SqlTransactionMode { get; set; }

        #endregion
    }
}
