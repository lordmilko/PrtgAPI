using System.Xml.Serialization;

namespace PrtgAPI.Objects.Undocumented
{
    /// <summary>
    /// Settings that apply to Devices within PRTG.
    /// </summary>
    public class DeviceSettings : DeviceOrGroupSettings
    {
        /*[XmlElement("injected_ipversion")]
        public IPVersion IPVersion { get; set; }

        [XmlElement("injected_host")]
        public string Host { get; set; }

        [XmlElement("injected_deviceicon")]
        public string Icon { get; set; }

        [XmlElement("injected_serviceurl")]
        public string ServiceUrl { get; set; }*/

        //name, status, ip version, ipv4 address, parent tags, tags, priority, device icon, service url, sensor management, discovery schedule

        //inherit snmp compatibility, snmp delay, failed requests, overflow values, zero values, 32-bit/64-bit counters, request mode, port name template, port identification, start interface index, end interface index
        //inherit http, name, port, user, password

        //then the scanning interval, schedules/dependencies/maintenance window, access rights, channel unit configuration and advanced network analysis settings
    }
}