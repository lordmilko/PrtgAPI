using System;
using System.Xml.Serialization;

namespace PrtgAPI
{
    public class SensorHistory
    {
        public DateTime DateTime { get; set; }

        public int SensorId { get; set; }

        [XmlAttribute("channel")]
        public string Name { get; set; }

        [XmlAttribute("channelid")]
        public int ChannelId { get; set; }

        /// <summary>
        /// Average value of the channel during the specified time period.
        /// </summary>
        [XmlText]
        public string Value { get; set; }
    }
}