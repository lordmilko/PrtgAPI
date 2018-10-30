using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PrtgAPI.Parameters;

namespace PrtgAPI.Request
{
    class VersionClient17_4 : VersionClient
    {
        internal VersionClient17_4(PrtgClient client) : base(RequestVersion.v17_4, client)
        {
        }

        internal VersionClient17_4(RequestVersion version, PrtgClient client) : base(version, client)
        {
        }

        internal override List<Location> ResolveAddressInternal(string address, CancellationToken token, bool lastAttempt)
        {
            var parameters = new ResolveAddressParameters(address, false);

            return GetAddressLocations(client.ObjectEngine.GetObject<HereOuterResponse>(
                parameters,
                ResponseParser.ResolveParser,
                token
            ));
        }

        internal override async Task<List<Location>> ResolveAddressInternalAsync(string address, CancellationToken token, bool lastAttempt)
        {
            var parameters = new ResolveAddressParameters(address, false);

            return GetAddressLocations(await client.ObjectEngine.GetObjectAsync<HereOuterResponse>(
                parameters,
                m => Task.FromResult(ResponseParser.ResolveParser(m)),
                token).ConfigureAwait(false)
            );
        }

        private List<Location> GetAddressLocations(HereOuterResponse response)
        {
            return response.Response.View.SelectMany(v => v.Results).Select(r => r.Location).Cast<Location>().ToList();
        }
    }
}
