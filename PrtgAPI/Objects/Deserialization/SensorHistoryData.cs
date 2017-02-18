using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace PrtgAPI
{
    class SensorHistoryData
    {
        [XmlElement("datetime_raw")]
        public DateTime DateTime { get; set; }

        [XmlElement("value")]
        public List<SensorHistory> Values { get; set; }
    }
}
