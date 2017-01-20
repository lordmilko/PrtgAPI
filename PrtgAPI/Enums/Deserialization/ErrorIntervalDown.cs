using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies the number of scanning intervals to wait before setting the sensor <see cref="SensorStatus.Down"/> when an error is reported. When an error clears, the sensor will go <see cref="SensorStatus.Up"/> immediately.
    /// </summary>
    public enum ErrorIntervalDown
    {
        /// <summary>
        /// Set the sensor to <see cref="SensorStatus.Down"/>  immediately.
        /// </summary>
        [XmlEnum("0")]
        DownImmediately,

        /// <summary>
        /// Set the sensor to <see cref="SensorStatus.Warning"/>, and then to <see cref="SensorStatus.Down"/>.
        /// </summary>
        [XmlEnum("1")]
        OneWarningThenDown,

        /// <summary>
        /// Set the sensor to <see cref="SensorStatus.Warning"/> for two intervals, and then to <see cref="SensorStatus.Down"/>.
        /// </summary>
        [XmlEnum("2")]
        TwoWarningsThenDown,

        /// <summary>
        /// Set the sensor to <see cref="SensorStatus.Warning"/> for three intervals, and then to <see cref="SensorStatus.Down"/>.
        /// </summary>
        [XmlEnum("3")]
        ThreeWarningsThenDown,

        /// <summary>
        /// Set the sensor to <see cref="SensorStatus.Warning"/> for four intervals, and then to <see cref="SensorStatus.Down"/>.
        /// </summary>
        [XmlEnum("4")]
        FourWarningsThenDown,

        /// <summary>
        /// Set the sensor to <see cref="SensorStatus.Warning"/> for five intervals, and then to <see cref="SensorStatus.Down"/>.
        /// </summary>
        [XmlEnum("5")]
        FiveWarningsThenDown
    }
}
