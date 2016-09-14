using System.ComponentModel;

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
        /// Retrieve the value of a <see cref="T:PrtgAPI.Property"/> for a specified PRTG Object.
        /// </summary>
        [Description("getobjectproperty.htm")]
        GetObjectProperty
    }
}
