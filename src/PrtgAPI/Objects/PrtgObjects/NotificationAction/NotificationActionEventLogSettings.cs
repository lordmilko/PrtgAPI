using System.Collections.Generic;
using System.Xml.Serialization;
using PrtgAPI.Utilities;

namespace PrtgAPI.NotificationActions
{
    /// <summary>
    /// Settings that apply to Event Log Notification Actions.
    /// </summary>
    [XmlRoot(NotificationAction.CategoryEventLog)]
    public class NotificationActionEventLogSettings : BaseNotificationActionSettings
    {
        /// <summary>
        /// The name of the event log to store events under.
        /// </summary>
        [XmlElement("injected_eventlogfile")]
        public EventLog LogName { get; set; }

        /// <summary>
        /// The source to display for the event. Applies only if <see cref="LogName"/> is <see cref="EventLog.Application"/>.
        /// </summary>
        [XmlElement("injected_sender")]
        public string EventSource { get; set; }

        /// <summary>
        /// The type of event to log.
        /// </summary>
        [XmlElement("injected_eventtype")]
        public EventLogType? EventType { get; set; }

        /// <summary>
        /// The message to display in the log.
        /// </summary>
        [XmlElement("injected_message")]
        public string Message { get; set; }

        internal override void ToString(List<object> targets)
        {
            if (LogName == EventLog.Application)
                targets.Add($"Log: {LogName.GetDescription()}, Source: {EventSource}, Type: {EventType}");
            else
                targets.Add($"Log: {LogName.GetDescription()}, Type: {EventType}");
        }
    }
}