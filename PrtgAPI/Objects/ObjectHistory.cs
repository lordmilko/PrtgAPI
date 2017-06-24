using System;
using System.Xml.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI
{
    public class ObjectHistory
    {
        public int ObjectId { get; set; }

        [XmlElement("datetime_raw")]
        [PropertyParameter(nameof(Property.DateTime))]
        public DateTime DateTime { get; set; }

        [XmlElement("user")]
        [PropertyParameter(nameof(Property.UserName))]
        public string UserName { get; set; }

        [XmlElement("message")]
        [PropertyParameter(nameof(Property.Message))]
        public string Message { get; set; }
    }
}