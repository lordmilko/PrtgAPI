using System.Xml.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI
{
    /// <summary>
    /// <para type="description">A device used to perform monitoring against a site or set of systems.</para>
    /// </summary>
    public class Probe : GroupOrProbe
    {
        /// <summary>
        /// Connected status of the probe.
        /// </summary>
        [XmlElement("condition_raw")]
        [PropertyParameter(Property.ProbeStatus)]
        [PropertyParameter(Property.Condition)]
        public ProbeStatus ProbeStatus { get; set; }
    }
}
