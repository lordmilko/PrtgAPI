using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies whether to reuse sessions for multiple sensor scans.
    /// </summary>
    public enum VMwareSessionMode
    {
        /// <summary>
        /// Reuse sessions for multiple scans.
        /// </summary>
        [XmlEnum("1")]
        ReuseSession,

        /// <summary>
        /// Create a new session for each scan. This increases network load and creates additional log entries on the target device.
        /// </summary>
        [XmlEnum("0")]
        CreateNewSession
    }
}
