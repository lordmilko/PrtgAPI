using System.Xml.Serialization;

namespace PrtgAPI
{
    enum TicketStatus
    {
        [XmlEnum("1")]
        Open
    }
}