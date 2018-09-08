using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies the number of notification triggers on a PRTG Object, as well as whether any triggers are inherited from parent objects
    /// </summary>
    public class NotificationTypes : IEquatable<NotificationTypes>
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
        /// Total number of triggers. This property is language indendent, and as such may not reflect the sum of all State/Threshold/Speed/Volume/Change Triggers
        /// </summary>
        public int TotalTriggers { get; }

        /// <summary>
        /// Whether notification triggers are inherited from a PRTG Object's parent object.
        /// </summary>
        public bool InheritTriggers { get; }

        private readonly string rawNotificationTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationTypes"/> class.
        /// </summary>
        /// <param name="rawNotificationTypes">Raw <see cref="Property.NotificationTypes"/> value returned from a PRTG Request.</param>
        public NotificationTypes(string rawNotificationTypes)
        {
            this.rawNotificationTypes = rawNotificationTypes;

            if (rawNotificationTypes != null)
            {
                if (rawNotificationTypes.StartsWith("Inherited"))
                    InheritTriggers = true;

                SetTriggerValue("State", v => StateTriggers = v);
                SetTriggerValue("Threshold", v => ThresholdTriggers = v);
                SetTriggerValue("Change", v => ChangeTriggers = v);
                SetTriggerValue("Speed", v => SpeedTriggers = v);
                SetTriggerValue("Volume", v => VolumeTriggers = v);

                TotalTriggers = GetTotalTriggers();
            }
        }

        private int GetTotalTriggers()
        {
            var matches = Regex.Matches(rawNotificationTypes, "\\d+ ");

            var count = matches.Cast<Match>().Select(m => Convert.ToInt32(Regex.Replace(m.Value, "(\\d+) ", "$1"))).Sum();

            return count;
        }

        private void SetTriggerValue(string type, Action<int> setter)
        {
            if (ContainsTriggers(type))
            {
                setter(GetTriggerValue(type));
            }
        }

        private bool ContainsTriggers(string name)
        {
            return Regex.Match(rawNotificationTypes, $"\\d {name}").Success;
        }

        private int GetTriggerValue(string type)
        {
            return Convert.ToInt32(Regex.Replace(rawNotificationTypes, $"(.*?)(\\d+)( {type}.*)", "$2"));
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Inheritance: {InheritTriggers}, State: {StateTriggers}, Threshold: {ThresholdTriggers}, Change: {ChangeTriggers}, Speed: {SpeedTriggers}, Volume: {VolumeTriggers}";
        }

        /// <summary>
        /// Returns a boolean indicating if the passed in object obj is
        /// Equal to this.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        [ExcludeFromCodeCoverage]
        public override bool Equals(object other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (other.GetType() != typeof(NotificationTypes))
                return false;

            return IsEqual((NotificationTypes) other);
        }

        /// <summary>
        /// Returns a boolean indicating if the passed in object obj is
        /// Equal to this.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(NotificationTypes other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return IsEqual(other);
        }

        private bool IsEqual(NotificationTypes other)
        {
            return rawNotificationTypes == other.rawNotificationTypes;
        }

        /// <summary>
        /// Returns a hash code for this object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var result = 0;

                result = (result * 409) ^ rawNotificationTypes.GetHashCode();

                return result;
            }
        }
    }
}
