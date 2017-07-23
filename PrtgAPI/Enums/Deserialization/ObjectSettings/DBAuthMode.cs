using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies the type of authentication PRTG uses to connect to a database server.
    /// </summary>
    public enum DBAuthMode
    {
        
        [XmlEnum("0")]
        Windows,

        /// <summary>
        /// Use credentials specific to the SQL Server.
        /// </summary>
        [XmlEnum("1")]
        SQL
    }
}
