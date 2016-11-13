using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Objects.Shared;

namespace PrtgAPI
{
    public class Channel : PrtgObject
    {
        // ################################## Sensors, Channel ##################################
        // There is a copy in both Sensor and Channel

        private string lastvalue;

        /// <summary>
        /// Last value of this sensor's primary channel. If this sensor's primary channel has been recently changed, the sensor may need to be paused and unpause (otherwise it may just display "No Data").
        /// </summary>
        [XmlElement("lastvalue")]
        [PropertyParameter(nameof(Property.LastValue))]
        [PSVisible(true)]
        public string LastValue
        {
            get { return lastvalue; }
            set { lastvalue = value == string.Empty ? null : value; }
        }
    }
}
