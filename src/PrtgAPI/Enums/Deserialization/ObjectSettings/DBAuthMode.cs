using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies the type of authentication PRTG uses to connect to a database server.
    /// </summary>
    public enum DBAuthMode
    {
        /// <summary>
        /// Use the Windows Credentials of the sensor's PRTG Device.
        /// </summary>
        [XmlEnum("0")]
        Windows,

        /// <summary>
        /// Use credentials specific to the SQL Server.
        /// </summary>
        [XmlEnum("1")]
        SQL
    }
}
