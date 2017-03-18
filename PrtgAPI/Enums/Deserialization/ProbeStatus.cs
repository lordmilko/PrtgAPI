using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Connection status of a PRTG Probe.
    /// </summary>
    public enum ProbeStatus
    {
        /// <summary>
        /// The probe is disconnected from the PRTG Core Server.
        /// </summary>
        [XmlEnum("0")]
        Disconnected,

        /// <summary>
        /// The probe is connected to the PRTG Core Server.
        /// </summary>
        [XmlEnum("2")]
        Connected
    }
}
