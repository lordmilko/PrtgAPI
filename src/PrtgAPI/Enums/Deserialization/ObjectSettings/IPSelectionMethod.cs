using System.Xml.Serialization;

namespace PrtgAPI
{
    enum IPSelectionMethod
    {
        [XmlEnum("0")]
        IPv4ClassCRange,

        [XmlEnum("1")]
        IPv4Addresses,

        [XmlEnum("2")]
        IPv4Subnet,

        [XmlEnum("3")]
        IPv4Octet,

        [XmlEnum("4")]
        IPv6Addresses,

        [XmlEnum("5")]
        ActiveDirectory
    }
}
