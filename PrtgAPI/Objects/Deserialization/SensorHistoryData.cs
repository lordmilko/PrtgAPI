using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace PrtgAPI
{
    public class SensorHistoryData
    {
        [XmlElement("datetime_raw")]
        public DateTime DateTime { get; set; }

        public int SensorId { get; set; }

        [XmlElement("value")]
        public List<ChannelHistoryRecord> ChannelRecords { get; set; }

        public override string ToString()
        {
            return DateTime.ToString();
        }
    }
}
