using System.Collections.Generic;
using System.Collections.ObjectModel;
using PrtgAPI.Linq;
using PrtgAPI.Parameters;

namespace PrtgAPI.Request
{
    class SetChannelPropertyTask
    {
        public ReadOnlyCollection<Channel> Channels { get; }

        public ChannelParameter[] Parameters { get; }

        public SetChannelPropertyTask(IList<Channel> channels, ChannelParameter[] parameters)
        {
            Channels = channels.ToReadOnly();
            Parameters = parameters;
        }
    }
}