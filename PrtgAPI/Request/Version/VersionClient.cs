using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrtgAPI.Parameters;

namespace PrtgAPI.Request
{
    internal class VersionClient
    {
        internal RequestVersion Version { get; set; }

        protected PrtgClient client;

        internal VersionClient(RequestVersion version, PrtgClient client)
        {
            Version = version;
            this.client = client;
        }

        internal virtual void SetChannelProperty(int[] sensorIds, int channelId, List<Channel> channels, ChannelParameter[] @params, Tuple<ChannelProperty, object> versionSpecific = null)
        {
            client.SetObjectProperty(new SetChannelPropertyParameters(sensorIds, channelId, @params, versionSpecific), sensorIds.Length);
        }

        internal virtual async Task SetChannelPropertyAsync(int[] sensorIds, int channelId, List<Channel> channels, ChannelParameter[] @params, Tuple<ChannelProperty, object> versionSpecific = null)
        {
            await client.SetObjectPropertyAsync(new SetChannelPropertyParameters(sensorIds, channelId, @params, versionSpecific), sensorIds.Length).ConfigureAwait(false);
        }
    }
}
