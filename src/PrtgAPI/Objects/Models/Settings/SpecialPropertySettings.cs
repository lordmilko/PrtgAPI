using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using PrtgAPI.Targets;

namespace PrtgAPI
{
    [ExcludeFromCodeCoverage]
    internal class SpecialPropertySettings
    {
        public string WindowsPassword { get; set; }

        public string LinuxPassword { get; set; }

        public string LinuxPrivateKey { get; set; }

        public string VMwarePassword { get; set; }

        public string SSHElevationPassword { get; set; }

        public string SNMPv3Password { get; set; }

        public string SNMPv3EncryptionKey { get; set; }

        public string DBPassword { get; set; }

        public string AmazonSecretKey { get; set; }

        [XmlElement("injected_inherittriggers")]
        public bool? InheritTriggers { get; set; }

        [XmlElement("injected_comments")]
        public string Comments { get; set; }

        public string Host { get; set; }

        public string ProxyPassword { get; set; }

        [XmlElement("injected_sqlquery")]
        public SqlServerQueryTarget SqlServerQuery { get; set; }

        [XmlElement("injected_executionmode")]
        public SqlProcessingMode SqlProcessingMode { get; set; }

        [XmlElement("injected_unitconfig")]
        public string ChannelUnit { get; set; }

        public string LocationName { get; set; }
    }
}
