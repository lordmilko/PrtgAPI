using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI
{
    /// <summary>
    /// <para type="description">Describes an event that has occurred to a <see cref="PrtgObject"/>.</para>
    /// </summary>
    [DebuggerDisplay("DateTime = {DateTime}, Name = {Name,nq}")]
    public class Log : IEventObject, ITableObject
    {
        /// <summary>
        /// ID of the <see cref="PrtgObject"/> this event applies to.
        /// </summary>
        [XmlElement("objid")]
        [PropertyParameter(Property.Id)]
        public int Id { get; set; }

        [ExcludeFromCodeCoverage]
        int IEventObject.ObjectId => Id;

        /// <summary>
        /// Name of the <see cref="PrtgObject"/> this event applies to.
        /// </summary>
        [XmlElement("name")]
        [PropertyParameter(Property.Name)]
        public string Name { get; set; }

        /// <summary>
        /// The date and time the event occurred.
        /// </summary>
        [XmlElement("datetime_raw")]
        [PropertyParameter(Property.DateTime)]
        public DateTime DateTime { get; set; }

        /// <summary>
        /// The parent of the object the event pertained to.
        /// </summary>
        [XmlElement("parent")]
        [PropertyParameter(Property.Parent)]
        public string Parent { get; set; }

        /// <summary>
        /// Type of log record this object contains.
        /// </summary>
        [XmlElement("status_raw")]
        [PropertyParameter(Property.Status)]
        public LogStatus Status { get; set; }

        /// <summary>
        /// Sensor the event pertained to (if applicable)
        /// </summary>
        [XmlElement("sensor")]
        [PropertyParameter(Property.Sensor)]
        public string Sensor { get; set; }

        /// <summary>
        /// Device the event pertained to, or the device of the affected sensor.
        /// </summary>
        [XmlElement("device")]
        [PropertyParameter(Property.Device)]
        public string Device { get; set; }

        /// <summary>
        /// Group the event pertained to, or the group of the affected device or sensor.
        /// </summary>
        [XmlElement("group")]
        [PropertyParameter(Property.Group)]
        public string Group { get; set; }

        /// <summary>
        /// Probe the event pertained to, or the probe of the affected group, device or sensor.
        /// </summary>
        [XmlElement("probe")]
        [PropertyParameter(Property.Probe)]
        public string Probe { get; set; }

        internal string message;

        /// <summary>
        /// Message or subject displayed on an object.
        /// </summary>
        [XmlElement("message_raw")]
        [PropertyParameter(Property.Message)]
        public string Message
        {
            get { return GetMessage(); }
            set { message = value; }
        }

        [XmlElement("message")]
        internal string DisplayMessage { get; set; }

        private string GetMessage()
        {
            if (message != null && Regex.Match(message, "#[a-zA-Z].+").Success && DisplayMessage.StartsWith("<div"))
            {
                return Regex.Replace(DisplayMessage, "(<div.+?>)(.+?)(<.+>)", "$2");
            }                

            return message;
        }

        /// <summary>
        /// <see cref="Priority"/> of this object.
        /// </summary>
        [XmlElement("priority")]
        [XmlElement("priority_raw")]
        [PropertyParameter(Property.Priority)]
        public Priority Priority { get; set; }

        /// <summary>
        /// The display type of the object this object pertains to.
        /// </summary>
        [XmlElement("type")]
        public string DisplayType { get; set; }

        /// <summary>
        /// The raw type name of the object this object pertains to.
        /// </summary>
        [XmlElement("type_raw")]
        internal string type { get; set; }

        private StringEnum<ObjectType> enumType;

        /// <summary>
        /// The type of this object this object pertains to.
        /// </summary>
        [PropertyParameter(Property.Type)]
        public StringEnum<ObjectType> Type
        {
            get
            {
                if (enumType == null && !string.IsNullOrEmpty(type))
                    enumType = new StringEnum<ObjectType>(type);

                return enumType;
            }
            set { enumType = value; }
        }

        /// <summary>
        /// Tags contained on the object this event pertains to.
        /// </summary>
        [XmlElement("tags")]
        [StandardSplittableString]
        [PropertyParameter(Property.Tags)]
        public string[] Tags { get; set; }

        /// <summary>
        /// Whether or not the object is currently active (in a monitoring state). If false, the object is paused.
        /// </summary>
        [XmlElement("active_raw")]
        [PropertyParameter(Property.Active)]
        public bool Active { get; set; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{DateTime}: {Name}";
        }
    }
}
