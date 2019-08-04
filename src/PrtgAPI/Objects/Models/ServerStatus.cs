using System;
using System.Runtime.Serialization;
using PrtgAPI.Request.Serialization;
using PrtgAPI.Utilities;

namespace PrtgAPI
{
#pragma warning disable CS0649
    /// <summary>
    /// <para type="description">Status details of a PRTG Server.</para>
    /// </summary>
    [DataContract]
    public class ServerStatus
    {
        [DataMember(Name = "Clock")]
        private string clockStr;

        private DateTime? clock;

        /// <summary>
        /// Current system time on the PRTG Core Server.
        /// </summary>
        public DateTime DateTime
        {
            get
            {
                if (clock == null)
                    clock = DateTime.Parse(clockStr);

                return clock.Value;
            }
        }

        [DataMember(Name = "Version")]
        private string versionStr;

        private Version version;

        /// <summary>
        /// Version of PRTG Network Monitor used by the server.
        /// </summary>
        public Version Version => version ?? (version = Version.Parse(versionStr.Trim('+')));

        /// <summary>
        /// Whether the PRTG Core is running as 64-bit.
        /// </summary>
        public bool Is64Bit => versionStr.EndsWith("+");

        /// <summary>
        /// The number of new unread log entries since last logon.
        /// </summary>
        [DataMember(Name = "NewMessages")]
        public int NewLogs { get; set; }

        /// <summary>
        /// The number of new unread red sensor alerts since last logon.
        /// </summary>
        [DataMember(Name = "NewAlarms")]
        public int NewAlarms { get; set; }

        [DataMember(Name = "Alarms")]
        private string alarmsStr;

        /// <summary>
        /// The number of sensors currently in a <see cref="Status.Down"/> state.
        /// </summary>
        public int Alarms => TypeHelpers.StrToInt(alarmsStr);

        [DataMember(Name = "AckAlarms")]
        private string acknowledgedAlarmsStr;

        /// <summary>
        /// The number of sensors currently in a <see cref="Status.DownAcknowledged"/> state.
        /// </summary>
        public int AcknowledgedAlarms => TypeHelpers.StrToInt(acknowledgedAlarmsStr);

        [DataMember(Name = "PartialAlarms")]
        private string partialAlarmsStr;

        /// <summary>
        /// The number of sensors currently in a <see cref="Status.DownPartial"/> state.
        /// </summary>
        public int PartialAlarms => TypeHelpers.StrToInt(partialAlarmsStr);

        [DataMember(Name = "UnusualSens")]
        private string unusualSensorsStr;

        /// <summary>
        /// The number of sensors currently in a <see cref="Status.Unusual"/> state.
        /// </summary>

        public int UnusualSensors => TypeHelpers.StrToInt(unusualSensorsStr);

        [DataMember(Name = "UpSens")]
        private string upSensorsStr;

        /// <summary>
        /// The number of sensors currently in a <see cref="Status.Up"/> state.
        /// </summary>

        public int UpSensors => TypeHelpers.StrToInt(upSensorsStr);

        [DataMember(Name = "WarnSens")]
        private string warningSensorsStr;

        /// <summary>
        /// The number of sensors currently in a <see cref="Status.Warning"/> state.
        /// </summary>
        public int WarningSensors => TypeHelpers.StrToInt(warningSensorsStr);

        [DataMember(Name = "PausedSens")]
        private string pausedSensorsStr;

        /// <summary>
        /// The number of sensors currently in a <see cref="Status.Paused"/> state.
        /// </summary>
        public int PausedSensors => TypeHelpers.StrToInt(pausedSensorsStr);

        [DataMember(Name = "UnknownSens")]
        private string unknownSensorsStr;

        /// <summary>
        /// The number of sensors currently in a <see cref="Status.Unknown"/> state.
        /// </summary>

        public int UnknownSensors => TypeHelpers.StrToInt(unknownSensorsStr);

        [DataMember(Name = "NewTickets")]
        private string newTicketsStr;

        /// <summary>
        /// The number of new unread tickets since last logon.
        /// </summary>
        public int NewTickets => TypeHelpers.StrToInt(newTicketsStr);

        private string userId;

        /// <summary>
        /// Object ID of your user within PRTG.
        /// </summary>
        [DataMember(Name = "UserId")]
        public string UserId
        {
            get { return userId; }
            set
            {
                if (value == string.Empty)
                    value = null;

                userId = value;
            }
        }

        private string userTimeZone;

        /// <summary>
        /// UTC offset of your PRTG Server's timezone.
        /// </summary>
        [DataMember(Name = "UserTimeZone")]
        public string UserTimeZone
        {
            get { return userTimeZone; }
            set
            {
                if (value == string.Empty)
                    value = null;

                userTimeZone = value;
            }
        }

        //[DataMember(Name = "ToDos")] //todo: what type is this
        //public string ToDos { get; set; } //todo: needs backing property to make it null

        /// <summary>
        /// Indicates how many sensors have been favorited.
        /// </summary>
        [DataMember(Name = "Favs")]
        public int Favorites { get; set; }
        
        /// <summary>
        /// The number of miscellaneous background tasks that are currently running.
        /// </summary>
        [DataMember(Name = "BackgroundTasks")]
        public int BackgroundTasks { get; set; }

        /// <summary>
        /// The number of similar sensor analysis tasks that are currently running.
        /// </summary>
        [DataMember(Name = "CorrelationTasks")]
        public int CorrelationTasks { get; set; }

        /// <summary>
        /// The number of Auto-Discovery tasks that are currently running.
        /// </summary>
        [DataMember(Name = "AutoDiscoTasks")]
        public int AutoDiscoveryTasks { get; set; }

        /// <summary>
        /// The number of scheduled report tasks that are currently running.
        /// </summary>
        [DataMember(Name = "ReportTasks")]
        public int ReportTasks { get; set; }

        [DataMember(Name = "EditionType")]
        private string licenseTypeStr;

        private LicenseType? licenseType;

        /// <summary>
        /// Whether PRTG has a Commercial License or is Freeware.
        /// </summary>
        public LicenseType LicenseType
        {
            get
            {
                if (licenseType == null)
                {
                    if (string.IsNullOrEmpty(licenseTypeStr))
                        licenseType = LicenseType.Freeware;
                    else
                        licenseType = licenseTypeStr.XmlToEnum<LicenseType>();
                }

                return licenseType.Value;
            }
        }
        
        /// <summary>
        /// Whether a PRTG Update is available for installation.
        /// </summary>
        [DataMember(Name = "PRTGUpdateAvailable")]
        public bool UpdateAvailable { get; set; }

        [DataMember(Name = "MaintExpiryDays")]
        private string maintenanceExpiryDays;

        /// <summary>
        /// Number of days until PRTG's licensed maintenance plan expires.
        /// </summary>
        public int? MaintenanceExpiryDays
        {
            get
            {
                if (maintenanceExpiryDays == "??" || maintenanceExpiryDays == "[Only visible for administrators]")
                    return null;

                return Convert.ToInt32(maintenanceExpiryDays);
            }
        }

        [DataMember(Name = "TrialExpiryDays")]
        private int trialExpiryDays;

        /// <summary>
        /// Number of days until or since your PRTG Trial expires/has expired. If a commercial license has been applied, this value is null.
        /// </summary>
        public int? TrialExpiryDays
        {
            get
            {
                if (trialExpiryDays == -999999)
                    return null;

                return trialExpiryDays;
            }
        }

        [DataMember(Name = "CommercialExpiryDays")]
        private string commercialExpiryDays;

        /// <summary>
        /// Number of days until your commercial license expires. In practice, commercial licenses do not expire.<para/>
        /// Equal to 1000 years + <see cref="MaintenanceExpiryDays"/> + 5. If you do not have a commercial license, this value is null.
        /// </summary>
        public int? CommercialExpiryDays
        {
            get
            {
                if (commercialExpiryDays == "-999999" || string.IsNullOrEmpty(commercialExpiryDays))
                    return null;

                return Convert.ToInt32(commercialExpiryDays);
            }
        }
        
        /// <summary>
        /// Whether the PRTG server is currently throttling user authentication requests due to excessive failed logons.
        /// </summary>
        [DataMember(Name = "Overloadprotection")]
        public bool OverloadProtection { get; set; }
        
        [DataMember(Name = "ClusterType")]
        private string clusterTypeStr;

        /// <summary>
        /// Indicates whether this node is the Master or a Failover Node in a PRTG Cluster. If this node is not part of a cluster, this value is null.
        /// </summary>
        public ClusterNodeType? ClusterNodeType
        {
            get
            {
                if (string.IsNullOrEmpty(clusterTypeStr))
                    return null;

                return clusterTypeStr.XmlToEnum<ClusterNodeType>();
            }
        }

        [DataMember(Name = "ClusterNodeName")]
        private string clusterNodeName;

        /// <summary>
        /// The name of this node in the PRTG Cluster. If this PRTG Server is not part of a PRTG Cluster, this value is null.
        /// </summary>
        public string ClusterNodeName
        {
            get
            {
                var start = "Cluster Node \"";

                if (clusterNodeName.StartsWith(start))
                    clusterNodeName = clusterNodeName.Substring(start.Length);

                var endMaster = "\" (Current Master)";

                if (clusterNodeName.EndsWith(endMaster))
                    clusterNodeName = clusterNodeName.Substring(0, clusterNodeName.Length - endMaster.Length);

                var endFailover = "\" (Failover Node)";

                if (clusterNodeName.EndsWith(endFailover))
                    clusterNodeName = clusterNodeName.Substring(0, clusterNodeName.Length - endFailover.Length);

                if (clusterNodeName == string.Empty)
                    return null;

                return clusterNodeName;
            }
        }

        /// <summary>
        /// Whether the current user is a member of a group granting administrative rights.
        /// </summary>
        [DataMember(Name = "IsAdminUser")]
        public bool IsAdminUser { get; set; }

        /// <summary>
        /// Indicates whether this PRTG Server is part of a PRTG Cluster.
        /// </summary>
        public bool IsCluster => ClusterNodeType != null;

        [DataMember(Name = "ReadOnlyUser")]
        private string readOnlyUserStr;

        /// <summary>
        /// Indicates whether the current user is restricted to read-only access to PRTG.
        /// </summary>
        public bool ReadOnlyUser => readOnlyUserStr.ToLower() == "true";

        [DataMember(Name = "TicketUser")]
        private string ticketUserStr;

        /// <summary>
        /// Indicates whether the current user has access to the PRTG Ticketing System.
        /// </summary>
        public bool TicketUser => ticketUserStr.ToLower() == "true";

        [DataMember(Name = "ReadOnlyAllowAcknowledge")]
        private string readOnlyAllowAcknowledgeStr;
        
        /// <summary>
        /// If this user is a <see cref="ReadOnlyUser"/>, indicates whether this user is allowed to acknowledge sensors.<para/>
        /// If this user is not a read only user, this value is false.
        /// </summary>
        public bool ReadOnlyAllowAcknowledge => readOnlyAllowAcknowledgeStr.ToLower() == "true";
        
        /// <summary>
        /// Whether the PRTG Core Server is currently running low on memory.
        /// </summary>
        [DataMember(Name = "LowMem")]
        public bool LowMemory { get; set; }

        //[DataMember(Name = "ActivationAlert")]
        //public bool ActivationAlert { get; set; } //todo: is this bool

        //todo: needs backing property to make it null

        /// <summary>
        /// Host ID of the PRTG Core Server.
        /// </summary>
        [DataMember(Name = "PRTGHost")]
        public string HostId { get; set; }

        [DataMember(Name = "MaxSensorCount")]
        private string maxSensorCountStr;

        /// <summary>
        /// Maximum number of sensors that can be created. If there is no maximum (as PRTG has an Unlimited license) this value is null.
        /// </summary>
        public int? MaxSensorCount
        {
            get
            {
                if (string.IsNullOrEmpty(maxSensorCountStr) || maxSensorCountStr == "unlimited")
                    return null;

                return Convert.ToInt32(maxSensorCountStr);
            }
        }

        [DataMember(Name = "Activated")]
        private string activatedStr;

        /// <summary>
        /// Whether PRTG's license has been activated.
        /// </summary>
        public bool? Activated
        {
            get
            {
                if (string.IsNullOrEmpty(activatedStr))
                    return null;

                return Convert.ToBoolean(Convert.ToInt32(activatedStr));
            }
        }
    }
#pragma warning restore CS0649
}
