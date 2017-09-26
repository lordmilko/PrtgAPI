using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Parameters
{
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
