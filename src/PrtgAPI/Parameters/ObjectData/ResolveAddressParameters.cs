using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    class ResolveAddressParameters : BaseParameters, IJsonParameters
    {
        JsonFunction IJsonParameters.Function => JsonFunction.GeoLocator;

        public ResolveAddressParameters(string address, bool apiKey)
        {
            this[Parameter.Custom] = new List<CustomParameter>
            {
                new CustomParameter("cache", "false"), //Literal string

                //PRTG 17.3 and earlier use 0 for Google even though they used to be able to make geolookup requests without a key
                new CustomParameter("dom", apiKey ? 0 : 2),

                //Replace spaces with + so they will be encoded into %2B to prevent issues with PRTG 18.2.41+
                new CustomParameter("path", address.Replace(" ", "+"))
            };
        }
    }
}
