using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Helpers;

namespace PrtgAPI.Objects.Shared
{
    /// <summary>
    /// Base class for objects that appear in tables.
    /// </summary>
    public class ObjectTable : PrtgObject
    {
        // ################################## All Object Tables ##################################       

        /// <summary>
        /// The type of this object. Certain objects may simply report their <see cref="BaseType"/>, while others may get more specific (e.g. a sensor of type "Ping").
        /// </summary>
        [XmlElement("type")]
        [PropertyParameter(nameof(Property.Type))]
        public string Type { get; set; }

        [XmlElement("type_raw")]
        internal string typeStr { get; set; }

        internal SensorTypeInternal? typeRaw => (SensorTypeInternal?) EnumHelpers.XmlToEnum<XmlEnumAttribute>(typeStr, typeof (SensorTypeInternal), false);

        /// <summary>
        /// Tags contained on this object.
        /// </summary>
        [XmlElement("tags")]
        [SplittableString(' ')]
        [PropertyParameter(nameof(Property.Tags))] //todo: give this some attribute we can use to decide to split it later
        public string[] Tags { get; set; }

        /// <summary>
        /// Whether or not the object is currently active (in a monitoring state). If false, the object is paused.
        /// </summary>
        [XmlElement("active_raw")]
        [PropertyParameter(nameof(Property.Active))]
        public bool Active { get; set; }
    }
}
