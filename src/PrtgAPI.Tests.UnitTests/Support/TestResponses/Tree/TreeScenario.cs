using System;
using PrtgAPI.Utilities;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    abstract class TreeScenario
    {
        protected int requestNum;

        public IWebResponse GetResponse(string address, string function)
        {
            var components = UrlUtilities.CrackUrl(address);
            Content content = components["content"].DescriptionToEnum<Content>();
            requestNum++;
            return GetResponse(address, content);
        }

        protected abstract IWebResponse GetResponse(string address, Content content);

        protected Exception UnknownRequest(string address)
        {
            return new NotImplementedException($"Don't know how to handle request #{requestNum}: {address}");
        }
    }
}
