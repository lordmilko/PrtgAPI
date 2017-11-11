using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies license types use by a PRTG Server.
    /// </summary>
    public enum LicenseType
    {
        /// <summary>
        /// PRTG is running a freeware license.
        /// </summary>
        [XmlEnum("F")]
        Freeware,

        /// <summary>
        /// PRTG is using a paid commercial license.
        /// </summary>
        [XmlEnum("C")]
        Commercial
    }
}
