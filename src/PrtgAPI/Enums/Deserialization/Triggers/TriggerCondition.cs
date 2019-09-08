using System.Xml.Serialization;
using PrtgAPI.Attributes;

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
        [XmlEnum("0")]
        [XmlEnumAlternateName("above")]
        Above,

        /// <summary>
        /// Trigger when the sensor's value is below the threshold.
        /// </summary>
        [XmlEnum("1")]
        [XmlEnumAlternateName("below")]
        Below,

        /// <summary>
        /// Trigger when the sensor's value is equal to the threshold.
        /// </summary>
        [XmlEnum("2")]
        [XmlEnumAlternateName("Equal to")]
        Equals,

        /// <summary>
        /// Trigger when the sensor's value is not equal to the threshold.
        /// </summary>
        [XmlEnum("3")]
        [XmlEnumAlternateName("Not Equal to")]
        NotEquals,

        /// <summary>
        /// Trigger whenever the sensor's value changes. For use in <see cref="TriggerType.Change"/> triggers only.
        /// </summary>
        [XmlEnum("change")]
        Change
    }
}
