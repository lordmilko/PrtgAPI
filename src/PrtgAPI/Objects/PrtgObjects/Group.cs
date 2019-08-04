using System.Xml.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI
{
    /// <summary>
    /// <para type="description">A group used to organize one or more groups or devices.</para>
    /// </summary>
    public class Group : GroupOrProbe, ISensorOrDeviceOrGroup
    {
        // ################################## Sensors, Devices, Groups ##################################
        //Also in Device because device must be derived from DeviceOrGroupOrProbe
        //Also in Sensor because sensor must be derived from SensorOrDeviceOrGroupOrProbe

        /// <summary>
        /// Probe that manages the execution of the sensors contained within this group's devices.
        /// </summary>
        [XmlElement("probe")]
        [PropertyParameter(Property.Probe)]
        public string Probe { get; set; }

        // ################################## Devices, Groups ##################################
        // There is a copy in both Device and Group

        /// <summary>
        /// Auto-discovery progress (if one is in progress). Otherwise, null.
        /// </summary>
        [XmlElement("condition")]
        [PropertyParameter(Property.Condition)]
        public string Condition { get; set; }
    }
}
