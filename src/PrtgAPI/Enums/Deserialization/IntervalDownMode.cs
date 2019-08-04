using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies the number of scanning intervals to wait before setting the sensor <see cref="Status.Down"/> when an error is reported. When an error clears, the sensor will go <see cref="Status.Up"/> immediately.
    /// </summary>
    public enum IntervalErrorMode
    {
        /// <summary>
        /// Set the sensor to <see cref="Status.Down"/> immediately.
        /// </summary>
        [XmlEnum("0")]
        DownImmediately,

        /// <summary>
        /// Set the sensor to <see cref="Status.Warning"/>, and then to <see cref="Status.Down"/>.
        /// </summary>
        [XmlEnum("1")]
        OneWarningThenDown,

        /// <summary>
        /// Set the sensor to <see cref="Status.Warning"/> for two intervals, and then to <see cref="Status.Down"/>.
        /// </summary>
        [XmlEnum("2")]
        TwoWarningsThenDown,

        /// <summary>
        /// Set the sensor to <see cref="Status.Warning"/> for three intervals, and then to <see cref="Status.Down"/>.
        /// </summary>
        [XmlEnum("3")]
        ThreeWarningsThenDown,

        /// <summary>
        /// Set the sensor to <see cref="Status.Warning"/> for four intervals, and then to <see cref="Status.Down"/>.
        /// </summary>
        [XmlEnum("4")]
        FourWarningsThenDown,

        /// <summary>
        /// Set the sensor to <see cref="Status.Warning"/> for five intervals, and then to <see cref="Status.Down"/>.
        /// </summary>
        [XmlEnum("5")]
        FiveWarningsThenDown
    }
}
