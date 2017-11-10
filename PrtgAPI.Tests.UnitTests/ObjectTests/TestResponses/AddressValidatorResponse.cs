using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses
{
    public class AddressValidatorResponse : MultiTypeResponse
    {
        private string str;

        public AddressValidatorResponse(string str)
        {
            this.str = str;
        }

        protected override IWebResponse GetResponse(ref string address, string function)
        {
            if (!address.Contains(str))
                Assert.Fail($"Address '{address}' did not contain '{str}'");

            return base.GetResponse(ref address, function);
        }
    }
}
