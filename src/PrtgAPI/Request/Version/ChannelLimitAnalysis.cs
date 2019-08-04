using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PrtgAPI.Request
{
    [DebuggerDisplay("LimitValue = {LimitValue}, Channels = {ChannelsStr,nq}, Property = {Property}")]
    class ChannelLimitAnalysis
    {
        public double? LimitValue { get; }

        public List<Channel> Channels { get; }

        public ChannelProperty Property { get; }

        private string ChannelsStr => string.Join(", ", Channels.Select(c => c.SensorId));

        public ChannelLimitAnalysis(double? limitValue, List<Channel> channels, ChannelProperty property)
        {
            LimitValue = limitValue;
            Channels = channels;
            Property = property;
        }
    }
}
