using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies how sensor factories should respond when a source sensor enters an error state.
    /// </summary>
    public enum FactoryErrorMode
    {
        /// <summary>
        /// Show <see cref="Status.Down"/> when one or more source sensors are in error.
        /// </summary>
        [XmlEnum("0")]
        ErrorOnError,

        /// <summary>
        /// Show <see cref="Status.Warning"/> when one or more sensors are in error.
        /// </summary>
        [XmlEnum("1")]
        WarnOnError,

        /// <summary>
        /// Determine whether to enter <see cref="Status.Down"/> according to a custom formula.
        /// </summary>
        [XmlEnum("2")]
        CustomFormula
    }
}
