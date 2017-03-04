using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Objects.Shared;

namespace PrtgAPI
{
    /// <summary>
    /// <para type="description">A group used to organize one or more devices.</para>
    /// </summary>
    public class Group : GroupOrProbe
    {
        // ################################## Sensors, Devices, Groups ##################################
        //Also in Device because device must be derived from DeviceOrGroupOrProbe
        //Also in Sensor because sensor must be derived from SensorOrDeviceOrGroupOrProbe

        /// <summary>
        /// Probe that manages the execution of the sensors contained within this group's devices.
        /// </summary>
        [XmlElement("probe")]
        [PropertyParameter(nameof(Property.Probe))]
        public string Probe { get; set; }
    }
}
