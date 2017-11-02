using System;
using System.ComponentModel;
using System.Xml.Linq;
using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Helpers;

namespace PrtgAPI.Objects.Undocumented
{
    //todo: move settings that are shared between all object types to objectsettings base class

    /// <summary>
    /// Settings that apply to Sensors within PRTG.
    /// </summary>
    public class SensorSettings : ObjectSettings
    {
        const string TimeFormat = "yyyy,MM,dd,HH,mm,ss";

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
        /// The name of this sensor.<para/>
        /// Corresponds to Basic Sensor Settings -> Name
        /// </summary>
        [XmlElement("injected_name")]
        public string Name { get; set; }

        /// <summary>
        /// Tags that are inherited from this objects parent.<para/>
        /// Corresponds to Basic Sensor Settings -> Parent Tags
        /// </summary>
        [SplittableString(' ')]
        [XmlElement("injected_parenttags")]
        public string[] ParentTags { get; set; }

        /// <summary>
        /// Tags that are defined on this object.<para/>
        /// Corresponds to Basic Sensor Settings -> Tags
        /// </summary>
        [SplittableString(' ')]
        [XmlElement("injected_tags")]
        public string[] Tags { get; set; }

        /// <summary>
        /// The priority of this sensor.<para/>
        /// Corresponds to Basic Sensor Settings -> Priority
        /// </summary>
        [XmlElement("injected_priority")]
        public Priority? Priority { get; set; }

        #endregion
        #region Channel Unit Configuration

        //todo: what it corresponds to
        /// <summary>
        /// Whether to inherit the Channel Unit Configuration settings from this sensor's parent object.<para/>
        /// Corresponds to Channel Unit Configuration -> Inherit Channel Unit.
        /// </summary>
        [XmlElement("injected_unitconfiggroup")]
        public bool? InheritChannelUnit { get; set; }

        //todo: the actual unit

        #endregion
        #region Debug Options

        /// <summary>
        /// How raw sensor results should be stored for debugging purposes. Corresponds to:<para/>
        ///     Debug Options (WMI) -> Sensor Result<para/>
        ///     Sensor Settings (EXE/XML) -> EXE Result
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
        #region Scanning Interval

        /// <summary>
        /// Whether to inherit Scanning Interval settings from the parent object.<para/>
        /// Corresponds to Scanning Interval -> Inherit Scanning Interval
        /// </summary>
        [XmlElement("injected_intervalgroup")]
        public bool? InheritInterval { get; set; }

        /// <summary>
        /// The raw interval of this object. This field is for internal use only.
        /// </summary>
        [XmlElement("injected_interval")]
        protected string intervalStr { get; set; }

        /// <summary>
        /// The backing field of this object's <see cref="Interval"/>. This field is for internal use only.
        /// </summary>
        protected ScanningInterval interval;

        /// <summary>
        /// The scanning interval of this object.<para/>
        /// Corresponds to Scanning Interval -> Scanning Interval.
        /// </summary>
        public ScanningInterval Interval
        {
            get
            {
                if (interval == null)
                    interval = ScanningInterval.Parse(intervalStr);

                return interval;
            }
        }


        //public ScanningInterval Interval { get; set; }  //todo: what if its a custom interval, how do we tell the deserializer how to deserialize it. use a parse method? shouldnt we be throwing right now though?
        //maybe we have an internal raw property and a public one that creates an object from it. need to store with a private field, and also change Schedule to do the same
        //what happens if you assign an invalid interval via set-objectproperty?

        /// <summary>
        /// How this sensor should react in the event of a sensor query failure.<para/>
        /// Corresponds to Scanning Interval -> If a Sensor Query Fails
        /// </summary>
        [XmlElement("injected_errorintervalsdown")]
        public IntervalErrorMode? IntervalErrorMode { get; set; }

        #endregion
        #region Schedules, Dependencies and Maintenance Window

        /// <summary>
        /// Whether to inherit Schedules, Dependencies and Maintenance Window settings from the parent object.<para/>
        /// Corresponds to Schedules, Dependencies and Maintenance Window -> Inherit Settings
        /// </summary>
        [XmlElement("injected_scheduledependency")]
        public bool? InheritDependency { get; set; }

        /// <summary>
        /// The raw schedule of this object. This field is for internal use only.
        /// </summary>
        [XmlElement("injected_schedule")]
        protected string scheduleStr { get; set; }

        private Lazy<Schedule> schedule;

        /// <summary>
        /// The schedule during which monitoring is active. If the schedule is not active, sensors will be <see cref="Status.PausedBySchedule"/>.<para/>
        /// Corresponds to Schedules, Dependencies and Maintenance Window -> Schedule.
        /// </summary>
        public Schedule Schedule
        {
            get
            {
                if (schedule == null)
                    schedule = new Lazy<Schedule>(() => new Schedule(scheduleStr));

                return schedule.Value;
            }
        }

        /// <summary>
        /// Whether a one-time maintenance window has been defined.<para/>
        /// Corresponds to Schedules, Dependencies and Maintenance Window -> Maintenance Window.
        /// </summary>
        [XmlElement("injected_maintenable")]
        public bool? MaintenanceEnabled { get; set; }

        /// <summary>
        /// The raw maintenance start of this object. This field is for internal use only.
        /// </summary>
        [XmlElement("injected_maintstart")]
        protected string maintenanceStartStr { get; set; }

        private Lazy<DateTime> maintenanceStart;

        /// <summary>
        /// The start time of a one-time maintenance window. If <see cref="MaintenanceEnabled"/> is false, this property will contain the default maintenance start value.<para/>
        /// Corresponds to Schedules, Dependencies and Maintenance Window -> Maintenance Begins.
        /// </summary>
        public DateTime MaintenanceStart
        {
            get
            {
                if (maintenanceStart == null)
                    maintenanceStart = new Lazy<DateTime>(() => DateTime.ParseExact(maintenanceStartStr, TimeFormat, null));

                return maintenanceStart.Value;
            }
        }

        /// <summary>
        /// The raw maintenance end of this object. This field is for internal use only.
        /// </summary>
        [XmlElement("injected_maintend")]
        protected string maintenanceEndStr { get; set; }

        private Lazy<DateTime> maintenanceEnd;

        /// <summary>
        /// The end time of a one-time maintenance window. If <see cref="MaintenanceEnabled"/> is false, this property will contain the default maintenance end value.<para/>
        /// Corresponds to Schedules, Dependencies and Maintenance Window -> Maintenance Ends.
        /// </summary>
        public DateTime MaintenanceEnd
        {
            get
            {
                if (maintenanceEnd == null)
                    maintenanceEnd = new Lazy<DateTime>(() => DateTime.ParseExact(maintenanceEndStr, TimeFormat, null));

                return maintenanceEnd.Value;
            }
        }

        /// <summary>
        /// Indicates the type of object this object uses as its dependency. When the dependency object goes down, this object is <see cref="Status.PausedByDependency"/>.<para/>
        /// Corresponds to Schedules, Dependencies and Maintenance Window -> Dependency Type.
        /// </summary>
        [XmlElement("injected_dependencytype")]
        public DependencyType? DependencyType { get; set; }

        /// <summary>
        /// The custom object to use as a dependency of this object.<para/>
        /// Corresponds to Schedules, Dependencies and Maintenance Window -> Dependency.
        /// </summary>
        [XmlElement("injected_dependencyvalue")]
        public string DependentObjectId { get; set; }

        /// <summary>
        /// Duration (in seconds) to delay resuming this sensor after its master object returns to <see cref="Status.Up"/>. This property only applies when <see cref="DependencyType"/> is <see cref="PrtgAPI.DependencyType.Object"/>.<para/>
        /// Corresponds to Schedules, Dependencies and Maintenance Window -> Dependency Delay.
        /// </summary>
        [XmlElement("injected_depdelay")]
        public int? DependencyDelay { get; set; }

        #endregion
        #region Sensor Display

        //[XmlElement("injected_primarychannel")]
        //public string PrimaryChannel { get; set; }

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

        //Duplicate properties: DebugMode, Timeout

        /// <summary>
        /// Name of the EXE or Script File the sensor executes.<para/>
        /// Corresponds to Sensor Settings -> EXE/Script
        /// </summary>
        [XmlElement("injected_exefile")]
        public string ExeName { get; set; }

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

        /// <summary>
        /// Whether change triggers defined on this sensor will be activated when this sensor's value changes.<para/>
        /// Corresponds to OID Values -> If Value Changes
        /// </summary>
        [XmlElement("injected_monitorchange")]
        public bool? EnableChangeTriggers { get; set; }

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
    }
}
