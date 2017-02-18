using System;
using System.Xml.Serialization;
using DH = PrtgAPI.Objects.Deserialization.DeserializationHelpers;

namespace PrtgAPI
{
    internal class ServerStatus
    {
        [XmlElement("NewMessages")]
        public int? NewMessages { get; set; }

        [XmlElement("NewAlarms")]
        public int? NewAlarms { get; set; }

        [XmlElement("Alarms")]
        public string Alarms { get; set; }

        [XmlElement("AckAlarms")]
        public string AcknowledgedAlarms { get; set; }

        [XmlElement("NewToDos")]
        public string NewToDos { get; set; }

        public DateTime Clock => DateTime.Parse(_RawClock);

        [XmlElement("Clock")]
        public string _RawClock { get; set; }

        [XmlElement("ActivationStatusMessage")]
        public string ActivationStatusMessage { get; set; }

        [XmlElement("BackgroundTasks")]
        public int BackgroundTasks { get; set; }

        [XmlElement("CorrelationTasks")]
        public int CorrelationTasks { get; set; }

        [XmlElement("AutoDiscoTasks")]
        public int AutoDiscoveryTasks { get; set; }

        [XmlElement("Version")]
        public string Version { get; set; }
        
        public bool UpdateAvailable => DH.YesNoToBool(_RawUpdateAvailable);

        [XmlElement("PRTGUpdateAvailable")]
        public string _RawUpdateAvailable { get; set; } //todo: remove all _raw fields from this file

        [XmlElement("IsAdminUser")]
        public bool IsAdminUser { get; set; }

        [XmlElement("IsCluster")]
        public bool IsCluster { get; set; }

        [XmlElement("ReadOnlyUser")]
        public bool ReadOnlyUser { get; set; } //is this bool?

        [XmlElement("ReadOnlyAllowAcknowledge")]
        public string ReadOnlyAllowAcknowledge { get; set; } //is this bool?

        [XmlElement("ReadOnlyPwChange")]
        public string ReadOnlyPwChange { get; set; } //is this bool?
    }
}
