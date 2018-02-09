using System;
using PrtgAPI.Helpers;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.TreeNodes;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses
{
    abstract class GroupScenario
    {
        protected int requestNum;
        protected ProbeNode probe;

        public IWebResponse GetResponse(string address, string function)
        {
            var components = UrlHelpers.CrackUrl(address);
            Content content = components["content"].ToEnum<Content>();
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