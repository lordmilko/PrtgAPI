using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies whether known devices should be re-auto-discovered when performing a group auto-discovery.
    /// </summary>
    internal enum DiscoveryRescanMode
    {
        /// <summary>
        /// Do not rescan existing devices.
        /// </summary>
        [XmlEnum("1")]
        SkipKnownIPs,

        //todo: need to find out if rescanning a group results in devices that already exist get recreated (and if you do it AGAIN theyre recreated for a third time)
        [XmlEnum("0")]
        RescanKnownIPs
    }
}
