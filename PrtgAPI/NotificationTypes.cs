using System;
using System.Text.RegularExpressions;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies the number of notification triggers on a PRTG Object, as well as whether any triggers are inherited from parent objects
    /// </summary>
    public class NotificationTypes
    {
        /// <summary>
        /// Number of State Triggers defined on a PRTG Object.
        /// </summary>
        public int StateTriggers { get; private set; }

        /// <summary>
        /// Number of Threshold Triggers defined on a PRTG Object.
        /// </summary>
        public int ThresholdTriggers { get; private set; }

        /// <summary>
        /// Number of Change Triggers defined on a PRTG Object.
        /// </summary>
        public int ChangeTriggers { get; private set; }

        /// <summary>
        /// Whether notification triggers are inherited from a PRTG Object's parent object.
        /// </summary>
        public bool TriggerInheritance { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:PrtgAPI.NotificationTypes"/> class.
        /// </summary>
        /// <param name="rawNotificationTypes">Raw <see cref="Property.NotifiesX"/> value returned from a PRTG Request.</param>
        public NotificationTypes(string rawNotificationTypes)
        {
            if (rawNotificationTypes != null)
            {
                if (rawNotificationTypes.StartsWith("Inherited"))
                    TriggerInheritance = true;
                if (Regex.Match(rawNotificationTypes, "\\d State").Success)
                {
                    StateTriggers = GetTriggerValue(rawNotificationTypes, "State");
                }

                if (Regex.Match(rawNotificationTypes, "\\d Threshold").Success)
                {
                    ThresholdTriggers = GetTriggerValue(rawNotificationTypes, "Threshold");
                }

                if (Regex.Match(rawNotificationTypes, "\\d Change").Success)
                {
                    ChangeTriggers = GetTriggerValue(rawNotificationTypes, "Change");
                }
            }
        }

        private int GetTriggerValue(string rawNotificationTypes, string type)
        {
            return Convert.ToInt32(Regex.Replace(rawNotificationTypes, $"(.*)(\\d)( {type}.*)", "$2"));
        }
    }
}
