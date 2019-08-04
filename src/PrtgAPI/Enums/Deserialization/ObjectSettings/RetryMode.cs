using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies whether or not to retry an action.
    /// </summary>
    public enum RetryMode
    {
        /// <summary>
        /// Retry the action.
        /// </summary>
        [XmlEnum("1")]
        Retry,

        /// <summary>
        /// Do not retry the action.
        /// </summary>
        [XmlEnum("0")]
        DoNotRetry
    }
}
