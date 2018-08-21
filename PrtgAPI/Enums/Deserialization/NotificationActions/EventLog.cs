using System.ComponentModel;
using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies the Event Log events shoud be placed under.
    /// </summary>
    public enum EventLog
    {
        /// <summary>
        /// Store events under the Application event log.
        /// </summary>
        [XmlEnum("application")]
        Application,

        /// <summary>
        /// Store events under the PRTG Network Monitor event log.
        /// </summary>
        [XmlEnum("internal")]
        [Description("PRTG Network Monitor")]
        PrtgNetworkMonitor
    }
}
