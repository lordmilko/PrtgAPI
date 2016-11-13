using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Parameters
{
    class ChannelParameters : ContentParameters<Channel>
    {
        public ChannelParameters(int sensorId) : base(Content.Channels)
        {
            this[Parameter.Id] = sensorId;
        }
    }
}
