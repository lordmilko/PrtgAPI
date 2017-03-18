using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Parameters
{
    class ChannelPropertiesParameters : Parameters
    {
        public ChannelPropertiesParameters(int sensorId, int channelId)
        {
            SensorId = sensorId;
            ChannelId = channelId;
        }

        public int SensorId
        {
            get { return (int)this[Parameter.Id]; }
            set { this[Parameter.Id] = value; }
        }

        public int ChannelId
        {
            get { return (int)this[Parameter.Channel]; }
            set { this[Parameter.Channel] = value; }
        }
    }
}
