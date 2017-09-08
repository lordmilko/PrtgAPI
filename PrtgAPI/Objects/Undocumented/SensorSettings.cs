using System;
using System.ComponentModel;
using System.Xml.Linq;
using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Helpers;

namespace PrtgAPI.Objects.Undocumented
{
    //todo: move settings that are shared between all object types to objectsettings base class

    //have a containersettings group for probe/group/device specific stuff

    //todo: need to implement dependencyproperties

    public class SensorSettings : ObjectSettings
    {
        const string TimeFormat = "yyyy,MM,dd,HH,mm,ss";

        #region Access Rights

        //todo: what does it correspond to
        /// <summary>
        /// Whether to inherit Access Rights settings from the parent object.
        /// </summary>
        [XmlElement("injected_accessgroup")]
        public bool? InheritAccess { get; set; }

        //todo: the actual access rights

        #endregion
        #region Basic Sensor Settings

        /// <summary>
        /// The name of this sensor. Corresponds to Basic Sensor Settings -> Name
        /// </summary>
        [XmlElement("injected_name")]
        public string Name { get; set; }

        /// <summary>
        /// Tags that are inherited from this objects parent. Corresponds to Basic Sensor Settings -> Parent Tags
        /// </summary>
        [SplittableString(' ')]
        [XmlElement("injected_parenttags")]
        public string[] ParentTags { get; set; }

        /// <summary>
        /// Tags that are defined on this object. Corresponds to Basic Sensor Settings -> Tags
        /// </summary>
        [SplittableString(' ')]
        [XmlElement("injected_tags")]
        public string[] Tags { get; set; }

        /// <summary>
        /// The priority of this sensor. Corresponds to Basic Sensor Settings -> Priority
        /// </summary>
        [XmlElement("injected_priority")]
        public Priority? Priority { get; set; }

        #endregion
        #region Channel Unit Configuration

        //todo: what it corresponds to
        /// <summary>
        /// Whether to inherit the Channel Unit Configuration settings from this sensor's parent object.
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

        [XmlElement("injected_httpurl")]
        public string Url { get; set; }

        [XmlElement("injected_httpmethod")]
        public HttpRequestMethod? HttpRequestMethod { get; set; }

        [XmlElement("injected_snihost")]
        public string SNI { get; set; }

        [XmlElement("injected_sni_inheritance")]
        public bool? UseSNIFromUrl { get; set; } //if true, the sni is derived from the Url. if false, the sni is inherited

        #endregion
        #region Ping Settings

        /// <summary>
        /// The duration (in seconds) this sensor can run for before timing out. Based on the sensor type, the maximum valid value may be different. Corresponds to:<para/>
        ///     Ping Settings                 -> Timeout (Max 300)<para/>
        ///     WMI Remote Ping Configuration -> Timeout (Max 300)<para/>
        ///     HTTP Specific                 -> Timeout (Max 900)
        /// </summary>
        [XmlElement("injected_timeout")]
        public int? Timeout { get; set; }

        [XmlElement("injected_size")]
        public int? PingPacketSize { get; set; } //note it can be between 1-10,000 bytes

        [XmlElement("injected_countmethod")]
        public PingMode? PingMode { get; set; }

        [XmlElement("injected_count")]
        public int? PingCount { get; set; }

        [XmlElement("injected_delay")]
        public int? PingDelay { get; set; }

        [XmlElement("injected_autoacknowledge")]
        public bool? AutoAcknowledge { get; set; }

        #endregion
        #region Scanning Interval

        /// <summary>
        /// Whether to inherit Scanning Interval settings from the parent object. Corresponds to Scanning Interval -> Inherit Scanning Interval
        /// </summary>
        [XmlElement("injected_intervalgroup")]
        public bool? InheritInterval { get; set; }

        [XmlElement("injected_interval")]
        protected string intervalStr { get; set; }

        protected ScanningInterval interval;

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

        [XmlElement("injected_schedule")]
        protected string scheduleStr { get; set; } //todo: remove all _raw fields in this file

        private Schedule schedule;

        internal Schedule Schedule
        {
            get
            {
                if (schedule == null)
                    schedule = new Schedule(scheduleStr);

                return schedule;
            }
        }

        [XmlElement("injected_maintenable")]
        public bool? MaintenanceEnabled { get; set; }

        public DateTime MaintenanceStart => DateTime.ParseExact(maintenanceStart, TimeFormat, null);

        [XmlElement("injected_maintstart")]
        protected string maintenanceStart { get; set; }

        public DateTime MaintenanceEnd => DateTime.ParseExact(maintenanceEnd, TimeFormat, null);

        [XmlElement("injected_maintend")]
        protected string maintenanceEnd { get; set; }

        [XmlElement("injected_dependencytype")]
        public DependencyType? DependencyType { get; set; } //if you select selectobject there is a second textbox we dont have that specifies that dependency object

        [XmlElement("injected_dependencyvalue")]
        public string DependentObjectId { get; set; }

        //todo: what does it correspond to
        /// <summary>
        /// Duration (in seconds) to delay resuming this sensor after its master object returns to <see cref="Status.Up"/>.
        /// </summary>
        [XmlElement("injected_depdelay")]
        public int? DependencyDelay { get; set; }

        #endregion
        #region Sensor Display

        //[XmlElement("injected_primarychannel")]
        //public string PrimaryChannel { get; set; }

        [XmlElement("injected_stack")]
        public GraphType GraphType { get; set; }

        [XmlElement("injected_stackunit")]
        internal string StackUnit { get; set; }

        #endregion
        #region Sensor Settings (EXE/XML)

        //Duplicate properties: DebugMode

        [XmlElement("injected_exefilelabel")]
        public string ExeName { get; set; }

        [XmlElement("injected_exeparams")]
        public string ExeParameters { get; set; }

        [XmlElement("injected_environment")]
        public bool? SetEnvironmentVariables { get; set; } //indicates whetehr the host, username, etc you pass in are available as environment variables

        [XmlElement("injected_usewindowsauthentication")]
        public bool? UseWindowsAuthentication { get; set; } //note that if false, the credentials of the probe service are used

        [XmlElement("injected_mutexname")]
        public string Mutex { get; set; }

        [XmlElement("injected_valuetype")]
        public ExeValueType? ExeValueType { get; set; }

        [XmlElement("injected_monitorchange")]
        public bool? EnableChangeTriggers { get; set; }

        #endregion
        #region WMI Alternative Query

        /// <summary>
        /// The method used for performing WMI queries. Corresponds to WMI Alternative Query -> Alternative Query
        /// </summary>
        [XmlElement("injected_wmialternative")]
        public WmiMode? WmiMode { get; set; }

        #endregion
        #region WMI Remote Ping Configuration

        //Duplicate properties: Timeout

        /// <summary>
        /// The DNS Name or IP Address to target. Corresponds to WMI Remote Ping Configuration -> Target.
        /// </summary>
        [XmlElement("injected_targetaddress")]
        public string Target { get; set; }

        [XmlElement("injected_buffersize")]
        public int? PingRemotePacketSize { get; set; } //note it can be between 1-10,000 bytes

        #endregion
    }
}
