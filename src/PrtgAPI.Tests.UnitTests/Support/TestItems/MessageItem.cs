namespace PrtgAPI.Tests.UnitTests.Support.TestItems
{
    public class MessageItem : BaseItem
    {
        public string Name { get; set; }
        public string DateTime { get; set; }
        public string DateTimeRaw { get; set; }
        public string Parent { get; set; }
        public string Status { get; set; }
        public string StatusRaw { get; set; }
        public string Sensor { get; set; }
        public string Device { get; set; }
        public string Group { get; set; }
        public string Probe { get; set; }
        public string Priority { get; set; }
        public string Message { get; set; }
        public string MessageRaw { get; set; }
        public string Type { get; set; }
        public string TypeRaw { get; set; }
        public string Tags { get; set; }
        public string Active { get; set; }
        public string ActiveRaw { get; set; }

        internal MessageItem(string name = "WMI Remote Ping", string datetime = "03.11.2017 05:10:31 PM", string datetimeRaw = "43042.2573037732", string parent = "Probe Device",
            string status = "Paused", string statusRaw = "604", string sensor = "WMI Remote Ping", string device = "Probe Device", string group = "Local Probe",
            string probe = "Local Probe", string priority = "1", string message = "<div class=\"logmessage\">Paused by user<div class=\"moreicon\"></div></div>",
            string messageRaw = "Paused by user", string objid = "2304", string type = "WMI Remote Ping",
            string typeRaw = "wmiremoteping", string tags = "pingsensor wmisensor wmipingsensor remotepingsensor", string active = "True", string activeRaw = "-1")
        {
            Name = name;
            DateTime = datetime;
            DateTimeRaw = datetimeRaw;
            Parent = parent;
            Status = status;
            StatusRaw = statusRaw;
            Sensor = sensor;
            Device = device;
            Group = group;
            Probe = probe;
            Priority = priority;
            Message = message;
            MessageRaw = messageRaw;
            ObjId = objid;
            Type = type;
            TypeRaw = typeRaw;
            Tags = tags;
            Active = active;
            ActiveRaw = activeRaw;
        }
    }
}
