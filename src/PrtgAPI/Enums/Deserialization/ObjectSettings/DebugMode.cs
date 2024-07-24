using System.Xml.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies whether to save debug information for troubleshooting sensor issues.
    /// </summary>
    public enum DebugMode
    {
        /// <summary>
        /// Discard all debug information.
        /// </summary>
        [XmlEnum("0")]
        [XmlEnumAlternateName("DiscardResult")]
        Discard,

        /// <summary>
        /// Save debug information to disk. Debug information is stored under C:\ProgramData\Paessler\PRTG Network Monitor\Logs (Sensors) on the sensors probe server.
        /// </summary>
        [XmlEnum("1")]
        [XmlEnumAlternateName("WriteResultToDisk")]
        WriteToDisk,

        /// <summary>
        /// Save debug information to disk if the scan results in an error. Not supported with WMI sensors. Debug information is stored under C:\ProgramData\Paessler\PRTG Network Monitor\Logs (Sensors) on the sensors probe server.
        /// </summary>
        [XmlEnum("2")]
        WriteToDiskWhenError
    }
}
