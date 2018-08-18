namespace PrtgAPI.Tests.UnitTests.Support.TestItems
{
    public class ServerStatusItem
    {
        public string NewMessages { get; set; }
        public string NewAlarms { get; set; }
        public string Alarms { get; set; }
        public string PartialAlarms { get; set; }
        public string AckAlarms { get; set; }
        public string UnusualSens { get; set; }
        public string UpSens { get; set; }
        public string WarnSens { get; set; }
        public string PausedSens { get; set; }
        public string UnknownSens { get; set; }
        public string NewTickets { get; set; }
        public string UserId { get; set; }
        public string UserTimeZone { get; set; }
        public string ToDos { get; set; }
        public string Favs { get; set; }
        public string Clock { get; set; }
        public string Version { get; set; }
        public string BackgroundTasks { get; set; }
        public string CorrelationTasks { get; set; }
        public string AutoDiscoTasks { get; set; }
        public string ReportTasks { get; set; }
        public string EditionType { get; set; }
        public string PrtgUpdateAvailable { get; set; }
        public string MaintExpiryDays { get; set; }
        public string TrialExpiryDays { get; set; }
        public string CommercialExpiryDays { get; set; }
        public string OverloadProtection { get; set; }
        public string ClusterType { get; set; }
        public string ClusterNodeName { get; set; }
        public string IsAdminUser { get; set; }
        public string ReadOnlyUser { get; set; }
        public string TicketUser { get; set; }
        public string ReadOnlyAllowAcknowledge { get; set; }
        public string LowMem { get; set; }
        public string ActivationAlert { get; set; }
        public string PrtgHost { get; set; }
        public string MaxSensorCount { get; set; }
        public string Activated { get; set; }

        public ServerStatusItem(string newMessages = "2890", string newAlarms = "2", string alarms = "42", string partialAlarms = "7",
            string ackAlarms = "6", string unusualSens = "2", string upSens = "388", string warnSens = "6", string pausedSens = "194",
            string unknownSens = "3", string newTickets = "2", string userId = "100", string userTimeZone = "UTC+8:00", string toDos = "",
            string favs = "7", string clock = "9/11/2017 10:18:02 PM", string version = "14.4.12.3283+", string backgroundTasks = "1",
            string correlationTasks = "1", string autoDiscoTasks = "2", string reportTasks = "1", string editionType = "C",
            string prtgUpdateAvailable = "true", string maintExpiryDays = "363", string trialExpiryDays = "3",
            string commercialExpiryDays = "365027", string overloadProtection = "true", string clusterType = "clustermaster",
            string clusterNodeName = "Cluster Node \\\"PRTG Network Monitor (Master)\\\" (Current Master)", string isAdminUser = "true",
            string readOnlyUser = "true", string ticketUser = "true", string readOnlyAllowAcknowledge = "true", string lowMem = "true",
            string activationAlert = "", string prtgHost = "12345678", string maxSensorCount = "100", string activated = "1")
        {
            NewMessages = newMessages;
            NewAlarms = newAlarms;
            Alarms = alarms;
            PartialAlarms = partialAlarms;
            AckAlarms = ackAlarms;
            UnusualSens = unusualSens;
            UpSens = upSens;
            WarnSens = warnSens;
            PausedSens = pausedSens;
            UnknownSens = unknownSens;
            NewTickets = newTickets;
            UserId = userId;
            UserTimeZone = userTimeZone;
            ToDos = toDos;
            Favs = favs;
            Clock = clock;
            Version = version;
            BackgroundTasks = backgroundTasks;
            CorrelationTasks = correlationTasks;
            AutoDiscoTasks = autoDiscoTasks;
            ReportTasks = reportTasks;
            EditionType = editionType;
            PrtgUpdateAvailable = prtgUpdateAvailable;
            MaintExpiryDays = maintExpiryDays;
            TrialExpiryDays = trialExpiryDays;
            CommercialExpiryDays = commercialExpiryDays;
            OverloadProtection = overloadProtection;
            ClusterType = clusterType;
            ClusterNodeName = clusterNodeName;
            IsAdminUser = isAdminUser;
            ReadOnlyUser = readOnlyUser;
            TicketUser = ticketUser;
            ReadOnlyAllowAcknowledge = readOnlyAllowAcknowledge;
            LowMem = lowMem;
            ActivationAlert = activationAlert;
            PrtgHost = prtgHost;
            MaxSensorCount = maxSensorCount;
            Activated = activated;
        }
    }
}
