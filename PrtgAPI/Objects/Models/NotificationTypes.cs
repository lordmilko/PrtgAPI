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
        /// Number of Speed Triggers defined on a PRTG Object.
        /// </summary>
        public int SpeedTriggers { get; private set; }

        /// <summary>
        /// Number of Volume Triggers defined on a PRTG Object.
        /// </summary>
        public int VolumeTriggers { get; private set; }

        /// <summary>
        /// Whether notification triggers are inherited from a PRTG Object's parent object.
        /// </summary>
        public bool InheritTriggers { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationTypes"/> class.
        /// </summary>
        /// <param name="rawNotificationTypes">Raw <see cref="Property.NotificationTypes"/> value returned from a PRTG Request.</param>
        public NotificationTypes(string rawNotificationTypes)
        {
            if (rawNotificationTypes != null)
            {
                if (rawNotificationTypes.StartsWith("Inherited"))
                    InheritTriggers = true;

                SetTriggerValue(rawNotificationTypes, "State", v => StateTriggers = v);
                SetTriggerValue(rawNotificationTypes, "Threshold", v => ThresholdTriggers = v);
                SetTriggerValue(rawNotificationTypes, "Change", v => ChangeTriggers = v);
                SetTriggerValue(rawNotificationTypes, "Speed", v => SpeedTriggers = v);
                SetTriggerValue(rawNotificationTypes, "Volume", v => SpeedTriggers = v);
            }
        }

        private void SetTriggerValue(string rawNotificationTypes, string type, Action<int> setter)
        {
            if (ContainsTriggers(rawNotificationTypes, type))
            {
                setter(GetTriggerValue(rawNotificationTypes, type));
            }
        }

        private bool ContainsTriggers(string rawNotificationTypes, string name)
        {
            return Regex.Match(rawNotificationTypes, $"\\d {name}").Success;
        }

        private int GetTriggerValue(string rawNotificationTypes, string type)
        {
            return Convert.ToInt32(Regex.Replace(rawNotificationTypes, $"(.*)(\\d)( {type}.*)", "$2"));
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Inheritance: {InheritTriggers}, State: {StateTriggers}, Threshold: {ThresholdTriggers}, Change: {ChangeTriggers}, Speed: {SpeedTriggers}, Volume: {VolumeTriggers}";
        }
    }
}
