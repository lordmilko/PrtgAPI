using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies the SSH engine to use.
    /// </summary>
    public enum SSHEngine
    {
        /// <summary>
        /// The default SSH engine.
        /// </summary>
        [XmlEnum("2")]
        Default,

        /// <summary>
        /// The compatibility mode engine (deprecated).
        /// </summary>
        [Obsolete]
        [XmlEnum("1")]
        CompatibilityMode
    }
}
