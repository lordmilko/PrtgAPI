using System;
using System.Net.Http.Headers;
using System.Text;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class LocationUnresolvedResponse : MultiTypeResponse, IWebContentHeaderResponse
    {
        private bool mapProviderUnavailable;

        public LocationUnresolvedResponse(bool mapProviderUnavailable = false)
        {
            this.mapProviderUnavailable = mapProviderUnavailable;
        }

        protected override IWebResponse GetResponse(ref string address, string function)
        {
            switch (function)
            {
                case nameof(JsonFunction.GeoLocator):
                    return GetEmptyLocationResponse();

                default:
                    return base.GetResponse(ref address, function);
            }
        }

        private IWebResponse GetEmptyLocationResponse()
        {
            if (mapProviderUnavailable)
            {
                HeaderAction = headers => headers.ContentType.MediaType = "image/png";
            }

            var builder = new StringBuilder();

            builder.Append("{");
            builder.Append("   \"results\" : []");
            builder.Append("}");

            return new BasicResponse(builder.ToString());
        }

        public Action<HttpContentHeaders> HeaderAction { get; set; }
    }
}
