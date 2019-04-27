using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI
{
    /// <summary>
    /// <para type="description">Represents a modification event of a PRTG Object.</para>
    /// </summary>
    [Description("Modification Event")]
    public class ModificationEvent : IEventObject
    {
        [ExcludeFromCodeCoverage]
        string IObject.Name => Message;

        /// <summary>
        /// The ID of the object the event occurred to.
        /// </summary>
        public int ObjectId { get; set; }

        /// <summary>
        /// The date/time the event was performed.
        /// </summary>
        [XmlElement("datetime_raw")]
        [PropertyParameter(Property.DateTime)]
        public DateTime DateTime { get; set; }

        /// <summary>
        /// The name of the user that performed the event.
        /// </summary>
        [XmlElement("user")]
        [PropertyParameter(Property.UserName)]
        public string UserName { get; set; }

        /// <summary>
        /// A description of the event.
        /// </summary>
        [XmlElement("message")]
        [PropertyParameter(Property.Message)]
        public string Message { get; set; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{DateTime}: {Message}";
        }
    }
}