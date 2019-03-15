using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Parameters;

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
        [XmlEnum("exexml")]
        [Type(typeof(ExeXmlSensorParameters))]
        ExeXml,

        /// <summary>
        /// WMI Service sensor, for monitoring a Microsoft Windows system service.
        /// </summary>
        [XmlEnum("wmiservice")]
        [NewSensor(DynamicName = true)]
        [Type(typeof(WmiServiceSensorParameters))]
        WmiService,

        /// <summary>
        /// Microsoft SQL v2 sensor, for monitoring a database on Microsoft SQL Server.
        /// </summary>
        [XmlEnum("mssqlv2")]
        [Type(null)]
        SqlServerDB,

        /// <summary>
        /// HTTP sensor, for monitoring a web server of HTTP/HTTPS.
        /// </summary>
        [NewSensor(ConfigOptional = true)]
        [Type(typeof(HttpSensorParameters))]
        Http,

        /// <summary>
        /// Sensor factory, for aggregating data from other sensors.
        /// </summary>
        [XmlEnum("aggregation")]
        [Type(typeof(FactorySensorParameters))]
        Factory
    }
}
