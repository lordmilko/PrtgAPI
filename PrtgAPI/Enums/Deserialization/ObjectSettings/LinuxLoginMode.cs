using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies the authentication mode to use when connecting to Linux Servers.
    /// </summary>
    public enum LinuxLoginMode
    {
        /// <summary>
        /// Authenticate with a Password.
        /// </summary>
        [XmlEnum("0")]
        Password,

        /// <summary>
        /// Authenticate with a Private Key.
        /// </summary>
        [XmlEnum("1")]
        PrivateKey
    }
}
