using System.Collections.Generic;
using System.Xml.Serialization;

namespace PrtgAPI.NotificationActions
{
    /// <summary>
    /// Settings that apply to Syslog Notification Actions.
    /// </summary>
    [XmlRoot(NotificationAction.CategorySyslog)]
    public class NotificationActionSyslogSettings : BaseNotificationActionSettings
    {
        /// <summary>
        /// IP Address/HostName of the syslog server.
        /// </summary>
        [XmlElement("injected_sysloghost")]
        public string Host { get; set; }

        /// <summary>
        /// Port to use on syslog server. Default port is 514.
        /// </summary>
        [XmlElement("injected_syslogport")]
        public int Port { get; set; }

        /// <summary>
        /// Logging facility to categorize the message under.
        /// </summary>
        [XmlElement("injected_syslogfacility")]
        public SyslogFacility Facility { get; set; }

        /// <summary>
        /// Encoding format to use for syslog message.
        /// </summary>
        [XmlElement("injected_syslogencoding")]
        public SyslogEncoding? Encoding { get; set; }

        /// <summary>
        /// Message to send to Syslog server.
        /// </summary>
        [XmlElement("injected_message")]
        public string Message { get; set; }

        internal override void ToString(List<object> targets)
        {
            targets.Add($"{Host}:{Port}");
        }
    }
}