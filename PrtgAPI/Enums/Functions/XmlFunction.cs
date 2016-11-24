using System.ComponentModel;
using PrtgAPI.Attributes;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies API Request Pages that return XML.
    /// </summary>
    public enum XmlFunction
    {
        /// <summary>
        /// Retrieve data stored by PRTG in tables (Sensors, Devices, Probes, etc).
        /// </summary>
        [Description("table.xml")]
        TableData,

        /// <summary>
        /// Retrieve the current system status.
        /// </summary>
        [Description("getstatus.xml")]
        GetStatus,

        /// <summary>
        /// Retrieve the value of a <see cref="Property"/> for a specified PRTG Object.
        /// </summary>
        [Description("getobjectproperty.htm")]
        GetObjectProperty,

        /// <summary>
        /// Get sensor totals (up, down, paused, etc) for a PRTG Server.
        /// </summary>
        [Description("gettreenodestats.xml")]
        GetTreeNodeStats
    }
}
