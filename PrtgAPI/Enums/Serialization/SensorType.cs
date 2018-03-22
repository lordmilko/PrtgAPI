using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// <para type="description">Specifies types of sensors that can be created in PRTG.</para>
    /// </summary>
    public enum SensorType
    {
        /// <summary>
        /// EXE/Script Advanced sensor, returning XML or JSON
        /// </summary>
        ExeXml,

        /// <summary>
        /// WMI Service sensor, for monitoring a Microsoft Windows system service.
        /// </summary>
        WmiService,

        /// <summary>
        /// Microsoft SQL v2 sensor, for monitoring a database on Microsoft SQL Server.
        /// </summary>
        [XmlEnum("mssqlv2")]
        SqlServerDB,

        /// <summary>
        /// HTTP sensor, for monitoring a web server of HTTP/HTTPS.
        /// </summary>
        Http
    }
}
