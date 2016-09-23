using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Parameters
{
    class SetChannelSettingParameters : SetObjectSettingParameters<ChannelProperty>
    {
        public SetChannelSettingParameters(int sensorId, int channelId, ChannelProperty property, string value) : base(sensorId, property, value)
        {
            ChannelId = channelId;
            SubType = SubType.Channel;
        }

        public int ChannelId
        {
            get { return (int) this[Parameter.SubId]; }
            set { this[Parameter.SubId] = value; }
        }

        public SubType SubType
        {
            get { return (SubType)this[Parameter.SubType]; }
            set { this[Parameter.SubType] = value; }
        }
    }
}
