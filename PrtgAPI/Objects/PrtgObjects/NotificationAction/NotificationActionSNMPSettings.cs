using System.Collections.Generic;
using System.Xml.Serialization;

namespace PrtgAPI.NotificationActions
{
    /// <summary>
    /// Settings that apply to SNMP Notification Actions.
    /// </summary>
    [XmlRoot(NotificationAction.CategorySNMP)]
    public class NotificationActionSNMPSettings : BaseNotificationActionSettings
    {
        //todo: if snmp isnt configured, these should all be null, right?

        /// <summary>
        /// The IP Address/HostName of the trap receiver server.
        /// </summary>
        [XmlElement("injected_snmphost")]
        public string Host { get; set; }

        /// <summary>
        /// Port to use on trap receiver server. Default port is 162.
        /// </summary>
        [XmlElement("injected_snmpport")]
        public int Port { get; set; }

        /// <summary>
        /// Community string to use for SNMP.
        /// </summary>
        [XmlElement("injected_snmpcommunity")]
        public string Community { get; set; }

        /// <summary>
        /// Trap code to use to identify the purpose of the trap.
        /// </summary>
        [XmlElement("injected_snmptrapspec")]
        public int TrapCode { get; set; }

        /// <summary>
        /// Message ID used to identify the origin of the trap. PRTG will send Message ID to trap server on OID 1.3.6.1.4.1.32446.1.1.1
        /// </summary>
        [XmlElement("injected_messageid")]
        public int MessageId { get; set; }

        /// <summary>
        /// Message to include in the SNMP trap.
        /// </summary>
        [XmlElement("injected_message")]
        public string Message { get; set; }

        /// <summary>
        /// IP Address to display as the sender. If this is blank, PRTG will use the IP of the PRTG Core Server.
        /// </summary>
        [XmlElement("injected_senderip")]
        public string SenderIP { get; set; }

        internal override void ToString(List<object> targets)
        {
            targets.Add($"{Host}:{Port}");
        }
    }
}