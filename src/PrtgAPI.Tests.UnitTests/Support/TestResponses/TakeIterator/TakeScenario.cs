using System;
using System.Xml.Linq;
using PrtgAPI.Utilities;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    abstract class TakeScenario
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

        protected static IWebResponse GetTotalLogsResponse()
        {
            return new BasicResponse(new XElement("messages",
                new XAttribute("listend", 1),
                new XAttribute("totalcount", 1000000),
                new XElement("prtg-version", "1.2.3.4"),
                null
            ).ToString());
        }
    }
}