using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Xml.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI
{
    /// <summary>
    /// Represents a modification event of a PRTG Object.
    /// </summary>
    [Description("Modification Event")]
    public class ModificationEvent : IEventObject
    {
        [ExcludeFromCodeCoverage]
        string IObject.Name
        {
            get { return Message; }
            set { Message = value; }
        }

        /// <summary>
        /// The ID of the object the event occurred to.
        /// </summary>
        public int ObjectId { get; set; }

        /// <summary>
        /// The date/time the event was performed.
        /// </summary>
        [XmlElement("datetime_raw")]
        [PropertyParameter(nameof(Property.DateTime))]
        public DateTime DateTime { get; set; }

        /// <summary>
        /// The name of the user that performed the event.
        /// </summary>
        [XmlElement("user")]
        [PropertyParameter(nameof(Property.UserName))]
        public string UserName { get; set; }

        /// <summary>
        /// A description of the event.
        /// </summary>
        [XmlElement("message")]
        [PropertyParameter(nameof(Property.Message))]
        public string Message { get; set; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{DateTime.ToString(CultureInfo.InvariantCulture)}: {Message}";
        }
    }
}