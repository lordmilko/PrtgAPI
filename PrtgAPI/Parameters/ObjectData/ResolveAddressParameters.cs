using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    class ResolveAddressParameters : Parameters
    {
        public ResolveAddressParameters(string address)
        {
            this[Parameter.Custom] = new List<CustomParameter>
            {
                new CustomParameter("cache", false),
                new CustomParameter("dom", 0),
                new CustomParameter("path", address)
            };
        }
    }
}
