using System.Collections.ObjectModel;
using PrtgAPI.Parameters;

namespace PrtgAPI.Request
{
    class SetChannelPropertyGrouping
    {
        public SetChannelPropertyParameters Parameters { get; }

        public ReadOnlyCollection<Channel> Channels { get; }

        public SetChannelPropertyGrouping(SetChannelPropertyParameters parameters, ReadOnlyCollection<Channel> channels)
        {
            Parameters = parameters;
            Channels = channels;
        }
    }
}