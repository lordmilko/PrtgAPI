using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        #region SetChannelProperty

        internal virtual void SetChannelProperty(int[] sensorIds, int channelId, List<Channel> channels, ChannelParameter[] @params, Tuple<ChannelProperty, object> versionSpecific = null)
        {
            client.SetObjectProperty(new SetChannelPropertyParameters(sensorIds, channelId, @params, versionSpecific), sensorIds.Length);
        }

        internal virtual async Task SetChannelPropertyAsync(int[] sensorIds, int channelId, List<Channel> channels, ChannelParameter[] @params, CancellationToken token, Tuple<ChannelProperty, object> versionSpecific = null)
        {
            await client.SetObjectPropertyAsync(new SetChannelPropertyParameters(sensorIds, channelId, @params, versionSpecific), sensorIds.Length, token).ConfigureAwait(false);
        }

        #endregion
        #region ResolveAddressInternal

        internal virtual List<Location> ResolveAddressInternal(string address, CancellationToken token, bool lastAttempt)
        {
            var parameters = new ResolveAddressParameters(address, true);

            var response = client.ObjectEngine.GetObject<GoogleGeoResult>(parameters, ResponseParser.ResolveParser, token);

            if (lastAttempt)
                HandleLastAttempt(response, address);

            return response.Results.Cast<Location>().ToList();
        }

        internal async virtual Task<List<Location>> ResolveAddressInternalAsync(string address, CancellationToken token, bool lastAttempt)
        {
            var parameters = new ResolveAddressParameters(address, true);

            var response = await client.ObjectEngine.GetObjectAsync<GoogleGeoResult>(parameters, m => Task.FromResult(ResponseParser.ResolveParser(m)), token).ConfigureAwait(false);

            if (lastAttempt)
                HandleLastAttempt(response, address);

            return response.Results.Cast<Location>().ToList();
        }

        private void HandleLastAttempt(GoogleGeoResult response, string address)
        {
            if(!string.IsNullOrEmpty(response.ErrorMessage))
            {
                throw new PrtgRequestException($"Could not resolve '{address}' to an actual address: server responded with '{response.ErrorMessage?.TrimEnd('.')}. {response.Status}'");
            }
        }

        #endregion
    }
}
