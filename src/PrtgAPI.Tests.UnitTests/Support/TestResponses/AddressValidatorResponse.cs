using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Request.Serialization;
using PrtgAPI.Utilities;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    public class AddressValidatorResponse : MultiTypeResponse
    {
        private string str;

        public string[] strArray;

        private bool exactMatch;

        private int arrayPos;

        private IWebResponse alternateResponse;

        public bool AllowReorder { get; set; }

        public bool AllowSecondDifference { get; set; }

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

#pragma warning disable 618
        public AddressValidatorResponse(string[] str, bool exactMatch, IWebResponse response) : this(str, exactMatch)
#pragma warning restore 618
        {
            alternateResponse = response;
        }

        [Obsolete("Do not create AddressValidatorResponse objects directly; use BaseTest.Execute instead")]
        public AddressValidatorResponse(object[] str) : this(str, true)
        {
        }

        protected override IWebResponse GetResponse(ref string address, string function)
        {
            ValidateAddress(address);

            if (alternateResponse != null)
                return new BasicResponse(alternateResponse.GetResponseText(ref address));
            else
                return base.GetResponse(ref address, function);
        }

        protected override IWebStreamResponse GetResponseStream(string address, string function)
        {
            ValidateAddress(address);

            if (alternateResponse != null)
                return new BasicResponse(alternateResponse.GetResponseText(ref address));
            else
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
                            {
                                if (!(AllowSecondDifference && AssertSecondDifferenceEqual(strArray[arrayPos], address)))
                                    throw GetDifference(strArray[arrayPos], address, ex);
                            }
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

        private bool AssertSecondDifferenceEqual(string expected, string actual)
        {
            var expectedParts = UrlUtilities.CrackUrl(expected);
            var actualParts = UrlUtilities.CrackUrl(actual);

            var differingParts = actualParts.AllKeys.Select(k => new
            {
                Expected = expectedParts[k],
                Actual = actualParts[k],
                Key = k
            }).Where(a => a.Expected != a.Actual).ToArray();

            if (differingParts.All(p =>
            {
                switch (p.Key)
                {
                    case "sdate":
                    case "edate":
                        var expectedDate = TypeHelpers.StringToDate(p.Expected);
                        var actualDate = TypeHelpers.StringToDate(p.Actual);

                        if (expectedDate.AddSeconds(1) == actualDate)
                            return true;

                        return false;
                    default:
                        return false;
                }
            }))
            {
                return true;
            }

            return false;
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
