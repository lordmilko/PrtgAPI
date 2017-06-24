using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace PrtgAPI
{
    internal class SensorHistoryData
    {
        [XmlElement("datetime_raw")]
        public DateTime DateTime { get; set; }

        public int SensorId { get; set; }

        [XmlElement("value")]
        public List<SensorHistory> Values { get; set; }

        public override string ToString()
        {
            return DateTime.ToString();
        }
    }
}
