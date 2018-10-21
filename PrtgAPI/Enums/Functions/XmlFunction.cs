using System.ComponentModel;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies API Request Pages that return XML.
    /// </summary>
    enum XmlFunction
    {
        /// <summary>
        /// Retrieve data stored by PRTG in tables (Sensors, Devices, Probes, etc).
        /// </summary>
        [Description("table.xml")]
        TableData,

        /// <summary>
        /// Retrieve the value of a <see cref="Property"/> for a specified PRTG Object.
        /// </summary>
        [Description("getobjectproperty.htm")]
        GetObjectProperty,

        /// <summary>
        /// Alternate method of retrieving the value of a <see cref="Property"/> for a specified PRTG Object. Does not support all <see cref="Property"/> values.
        /// </summary>
        [Description("getobjectstatus.htm")]
        GetObjectStatus,

        /// <summary>
        /// Retrieve the total number sensors in each sensor status (up, down, paused, etc) on a PRTG Server.
        /// </summary>
        [Description("gettreenodestats.xml")]
        GetTreeNodeStats,
        
        /// <summary>
        /// Retrieve historic channel data over a custom timeframe for a PRTG Sensor.
        /// </summary>
        [Description("historicdata.xml")]
        HistoricData
    }
}
