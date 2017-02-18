using System.ComponentModel;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies threshold conditions to be used in notification triggers.
    /// </summary>
    public enum TriggerCondition
    {
        /// <summary>
        /// Trigger when sensor's value is above the threshold.
        /// </summary>
        [Description("above")]
        Above,

        /// <summary>
        /// Trigger when the sensor's value is below the threshold.
        /// </summary>
        [Description("below")]
        Below,

        /// <summary>
        /// Trigger when the sensor's value is equal to the threshold.
        /// </summary>
        [Description("Equal to")]
        Equals,

        /// <summary>
        /// Trigger when the sensor's value is not equal to the threshold.
        /// </summary>
        [Description("Not Equal to")]
        NotEquals,

        /// <summary>
        /// Trigger whenever the sensor's value changes. For use in <see cref="TriggerType.Change"/> triggers only.
        /// </summary>
        [Description("change")]
        Change
    }
}
