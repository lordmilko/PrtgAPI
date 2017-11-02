using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies the calculation mode to use when a sensor used by a channel is paused or does not exist.
    /// </summary>
    public enum FactoryMissingDataMode
    {
        /// <summary>
        /// Do not calculate values for channels that use the affected sensor.
        /// </summary>
        [XmlEnum("0")]
        DontCalculate,

        /// <summary>
        /// Calculate values for channels that used the affected sensor using a value of "0" for any missing sensors.
        /// </summary>
        [XmlEnum("1")]
        CalculateWithZero
    }
}
