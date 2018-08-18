using System.Collections.Generic;
using System.Xml.Serialization;

namespace PrtgAPI.NotificationActions
{
    /// <summary>
    /// Settings that apply to Amazon Notification Actions.
    /// </summary>
    [XmlRoot(NotificationAction.CategoryAmazon)]
    public class NotificationActionAmazonSettings : BaseNotificationActionSettings
    {
        /// <summary>
        /// Access key to use for authentication to AWS.
        /// </summary>
        [XmlElement("injected_accesskeyid")]
        public string AccessKey { get; set; }

        /// <summary>
        /// Secret key to use for authentication to AWS.
        /// </summary>
        [XmlElement("injected_secretaccesskeyid")]
        public string SecretKey { get; set; }

        /// <summary>
        /// Region your AWS service is located in.
        /// </summary>
        [XmlElement("injected_amazonregion")]
        public string Region { get; set; }

        /// <summary>
        /// Amazon Resource Name to connect to.
        /// </summary>
        [XmlElement("injected_arn")]
        public string ResourceName { get; set; }

        /// <summary>
        /// Subject to display in the notification.
        /// </summary>
        [XmlElement("injected_subject")]
        public string Subject { get; set; }

        /// <summary>
        /// Message to include in the notification.
        /// </summary>
        [XmlElement("injected_message")]
        public string Message { get; set; }

        internal override void ToString(List<object> targets)
        {
            targets.Add(Subject);
        }
    }
}