using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Settings that apply to Devices within PRTG.
    /// </summary>
    public class DeviceSettings : DeviceOrGroupSettings
    {
        //todo: have some tests for these

        /// <summary>
        /// The internet protocol version to use to connect to the device.<para/>
        /// Corresponds to Basic Device Settings -> IP Version.
        /// </summary>
        [XmlElement("injected_ipversion")]
        public IPVersion IPVersion { get; set; }

        /// <summary>
        /// The IP Address or Hostname used to connect to the device.<para/>
        /// If the <see cref="IPVersion"/> of this device is <see cref="PrtgAPI.IPVersion.IPv6"/>, this returns the value of <see cref="Hostv6"/>. Otherwise, returns <see cref="Hostv4"/>.
        /// </summary>
        public string Host => IPVersion == PrtgAPI.IPVersion.IPv4 ? Hostv4 : Hostv6;

        /// <summary>
        /// The IPv4 Address or Hostname used to connect to the device.<para/>
        /// Corresponds to Basic Device Settings -> IPv4 Address/DNS Name.
        /// </summary>
        [XmlElement("injected_host")]
        public string Hostv4 { get; set; }

        /// <summary>
        /// The IPv6 Address or Hostname used to connect to the device.<para/>
        /// Corresponds to Basic Device Settings -> IPv6 Address/DNS Name.
        /// </summary>
        [XmlElement("injected_hostv6")]
        public string Hostv6 { get; set; }

        /*[XmlElement("injected_deviceicon")]
        public string Icon { get; set; }*/

        /// <summary>
        /// The URL used to service this device.<para/>
        /// Corresponds to Additional Device Information -> Service URL.
        /// </summary>
        [XmlElement("injected_serviceurl")]
        public string ServiceUrl { get; set; }

        //name, status, ip version, ipv4 address, parent tags, tags, priority, device icon, service url, sensor management, discovery schedule

        //inherit snmp compatibility, snmp delay, failed requests, overflow values, zero values, 32-bit/64-bit counters, request mode, port name template, port identification, start interface index, end interface index
        //inherit http, name, port, user, password

        //then the scanning interval, schedules/dependencies/maintenance window, access rights, channel unit configuration and advanced network analysis settings
    }
}