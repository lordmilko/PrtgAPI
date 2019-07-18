using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Utilities;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    public class AddressValidatorResponse : MultiTypeResponse
    {
        private string str;

        public string[] strArray;

        private bool exactMatch;

        private int arrayPos;

        public bool AllowReorder { get; set; }

        [Obsolete("Do not create AddressValidatorResponse objects directly; use BaseTest.Execute instead")]
        public AddressValidatorResponse(string str)
        {
            if (string.IsNullOrEmpty(str))
                throw new ArgumentException("At least one address must be specified", nameof(str));

            this.str = str;
            exactMatch = false;
        }

        [Obsolete("Do not create AddressValidatorResponse objects directly; use BaseTest.Execute instead")]
        public AddressValidatorResponse(object str, bool exactMatch)
        {
            if (string.IsNullOrEmpty(str?.ToString()))
                throw new ArgumentException("At least one address must be specified", nameof(str));

            if (str is object[])
                strArray = ((object[])str).Cast<string>().ToArray();
            else
                this.str = (string)str;

            this.exactMatch = exactMatch;

#if NETCOREAPP
            AllowReorder = true;
#endif
        }

        [Obsolete("Do not create AddressValidatorResponse objects directly; use BaseTest.Execute instead")]
        public AddressValidatorResponse(object[] str) : this(str, true)
        {
        }

        protected override IWebResponse GetResponse(ref string address, string function)
        {
            ValidateAddress(address);

            return base.GetResponse(ref address, function);
        }

        protected override IWebStreamResponse GetResponseStream(string address, string function)
        {
            ValidateAddress(address);

            return base.GetResponseStream(address, function);
        }

        internal void ValidateAddress(string address)
        {
            if (exactMatch)
            {
                if (strArray != null)
                {
                    if (arrayPos >= strArray.Length)
                    {
                        Assert.Fail($"Request for address '{address}' was not expected");
                    }

                    if (address != strArray[arrayPos])
                    {
                        try
                        {
                            Assert.AreEqual(strArray[arrayPos], address, "Address was not correct");
                        }
                        catch (AssertFailedException ex)
                        {
                            if (AllowReorder)
                                AssertEx.UrlsEquivalent(strArray[arrayPos], address);
                            else
                                throw GetDifference(strArray[arrayPos], address, ex);
                        }
                    }

                    arrayPos++;
                }
                else
                {
                    arrayPos++;
                    Assert.AreEqual(str, address, "Address was not correct");
                }
            }
            else
            {
                arrayPos++;

                if (!address.Contains(str))
                    Assert.Fail($"Address '{address}' did not contain '{str}'");
            }
        }

        private AssertFailedException GetDifference(string expected, string actual, AssertFailedException originalException)
        {
            var expectedParts = UrlUtilities.CrackUrl(expected);
            var actualParts = UrlUtilities.CrackUrl(actual);

            foreach(var part in actualParts.AllKeys)
            {
                var expectedVal = expectedParts[part];
                var actualVal = actualParts[part];

                if (expectedVal != actualVal)
                    Assert.Fail($"{part} was different. Expected: {expectedVal}. Actual: {actualVal}.{Environment.NewLine}{Environment.NewLine}{originalException.Message}");
            }

            return originalException;
        }

        public void AssertFinished()
        {
            if (strArray != null)
            {
                if (arrayPos < strArray.Length )
                    Assert.Fail($"Failed to call request '{strArray[arrayPos]}'");
            }
            else
            {
                if (arrayPos == 0)
                    Assert.Fail($"Failed to call request '{str}'");
            }
        }
    }
}
