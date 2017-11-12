using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses
{
    class LocationUnresolvedResponse : MultiTypeResponse
    {
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
            var builder = new StringBuilder();

            builder.Append("{");
            builder.Append("   \"results\" : []");
            builder.Append("}");

            return new BasicResponse(builder.ToString());
        }
    }
}
