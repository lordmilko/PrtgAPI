using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies whether an item is visible or not.
    /// </summary>
    public enum Visibility
    {
        /// <summary>
        /// The item is not visible.
        /// </summary>
        [XmlEnum("0")]
        NotVisible,

        /// <summary>
        /// The item is visible.
        /// </summary>
        [XmlEnum("1")]
        Visible
    }
}
