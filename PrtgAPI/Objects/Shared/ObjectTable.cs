using System;
using System.Xml.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI.Objects.Shared
{
    /// <summary>
    /// Properties that apply to objects found in tables.
    /// </summary>
    public class ObjectTable : PrtgObject
    {
        // ################################## All Object Tables ##################################

        /// <summary>
        /// ID number used to uniquely identify this object within PRTG.
        /// </summary>
        [XmlElement("objid")]
        [PropertyParameter(nameof(Property.ObjId))]
        public int? Id { get; set; }

        /// <summary>
        /// The type of this object. Certain objects may simply report their <see cref="BaseType"/>, while others may get more specific (e.g. a sensor of type "Ping").
        /// </summary>
        [XmlElement("type")]
        [PropertyParameter(nameof(Property.Type))]
        public string Type { get; set; }

        /// <summary>
        /// Name of this object.
        /// </summary>
        [XmlElement("name")]
        [PropertyParameter(nameof(Property.Name))]
        public string Name { get; set; }

        /// <summary>
        /// Tags contained on this object.
        /// </summary>
        [XmlElement("tags")]
        [PropertyParameter(nameof(Property.Tags))]
        public string Tags { get; set; }

        /// <summary>
        /// Whether or not the object is currently active (in a monitoring state). If false, the object is paused.
        /// </summary>
        [PropertyParameter(nameof(Property.Active))]
        public bool? Active => Convert.ToBoolean(_RawActive);

        private string activeraw;

        /// <summary>
        /// Raw value used for <see cref="Active"/> attribute. This property should not be used.
        /// </summary>
        [XmlElement("active")]
        public string _RawActive
        {
            get { return activeraw; }
            set { activeraw = Convert.ToBoolean(value).ToString(); }
        }
    }
}
