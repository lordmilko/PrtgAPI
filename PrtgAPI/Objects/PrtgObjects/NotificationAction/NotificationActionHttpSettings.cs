using System.Collections.Generic;
using System.Xml.Serialization;

namespace PrtgAPI.NotificationActions
{
    /// <summary>
    /// Settings that apply to HTTP Notification Actions.
    /// </summary>
    [XmlRoot(NotificationAction.CategoryHttp)]
    public class NotificationActionHttpSettings : BaseNotificationActionSettings
    {
        /// <summary>
        /// URL PRTG should execute a HTTP request against.
        /// </summary>
        [XmlElement("injected_url")]
        public string Url { get; set; }

        /// <summary>
        /// Whether PRTG should send the Server Name Indication when executing the <see cref="Url"/>.
        /// </summary>
        [XmlElement("injected_urlsniselect")]
        public bool? SendSNI { get; set; }

        /// <summary>
        /// Server Name Indication required by target <see cref="Url"/>.
        /// </summary>
        [XmlElement("injected_urlsniname")]
        public string SNIName { get; set; }

        /// <summary>
        /// POST data to include in the HTTP request. Can include variable placeholders.
        /// </summary>
        [XmlElement("injected_postdata")]
        public string PostData { get; set; }

        internal override void ToString(List<object> targets)
        {
            targets.Add(Url);
        }
    }
}