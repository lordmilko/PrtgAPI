using System.Collections.Generic;
using System.Xml.Serialization;

namespace PrtgAPI.NotificationActions
{
    /// <summary>
    /// Base class for notification settings that can be defined on a <see cref="NotificationAction"/>.
    /// </summary>
    public abstract class BaseNotificationActionSettings
    {
        /// <summary>
        /// Whether this notification type is enabled on its parent <see cref="NotificationAction"/>.
        /// </summary>
        [XmlElement("injected_active")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            if (Enabled)
            {
                var targets = new List<object>();

                ToString(targets);

                if (targets.Count == 0)
                    targets.Add("None");

                return string.Join(", ", targets);
            }

            return "None (Disabled)";
        }

        internal bool IsSet(object obj)
        {
            return obj != null && obj.ToString() != "None";
        }

        internal abstract void ToString(List<object> targets);
    }
}
