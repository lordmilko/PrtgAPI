using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses
{
    public class AddressValidatorResponse : MultiTypeResponse
    {
        private string str;

        public string[] strArray;

        private bool exactMatch;

        private int arrayPos;

        public AddressValidatorResponse(string str)
        {
            this.str = str;
            exactMatch = false;
        }

        public AddressValidatorResponse(object str, bool exactMatch)
        {
            if (str is object[])
                strArray = ((object[])str).Cast<string>().ToArray();
            else
                this.str = (string)str;

            this.exactMatch = exactMatch;
        }

        public AddressValidatorResponse(object[] str) : this(str, true)
        {
        }

        protected override IWebResponse GetResponse(ref string address, string function)
        {
            if (exactMatch)
            {
                if (strArray != null)
                {
                    if (arrayPos >= strArray.Length)
                    {
                        Assert.Fail($"Request for address '{address}' was not expected");
                    }

                    if(address != strArray[arrayPos])
                        Assert.AreEqual(strArray[arrayPos], address, "Address was not correct");

                    arrayPos++;
                }
                else
                {
                    Assert.AreEqual(str, address, "Address was not correct");
                }
            }
            else
            {
                if (!address.Contains(str))
                    Assert.Fail($"Address '{address}' did not contain '{str}'");
            }

            return base.GetResponse(ref address, function);
        }
    }
}
