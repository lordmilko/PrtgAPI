using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Request;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    public class ReadOnlyResponse : IWebResponse
    {
        IWebResponse normalResponse;

        public ReadOnlyResponse(IWebResponse normalResponse)
        {
            this.normalResponse = normalResponse;
        }

        public string GetResponseText(ref string address)
        {
            var func = MultiTypeResponse.GetFunctionEnum(address);

            if (func.Equals(CommandFunction.AddSensor2))
                return string.Empty;

            var htmlFunc = func as HtmlFunction?;

            var response = normalResponse.GetResponseText(ref address);

            if (htmlFunc != null)
                return CleanResponse(response);

            return response;
        }

        private string CleanResponse(string response)
        {
            response = Regex.Replace(response, HtmlParser.DefaultBasicMatchRegex, string.Empty, RegexOptions.Singleline);
            response = Regex.Replace(response, HtmlParser.DefaultBackwardsMatchRegex, string.Empty, RegexOptions.Singleline);
            response = Regex.Replace(response, HtmlParser.DefaultTextAreaRegex, string.Empty, RegexOptions.Singleline);
            response = Regex.Replace(response, HtmlParser.DefaultDropDownListRegex, string.Empty, RegexOptions.Singleline);
            response = Regex.Replace(response, HtmlParser.DefaultDependencyDiv, string.Empty, RegexOptions.Singleline);

            var xml = HtmlParser.Default.GetXml(new PrtgResponse(response, false));

            var descendents = xml.Descendants().ToList();

            Assert.AreEqual(0, descendents.Count, string.Join(", ", descendents.Select(d => d.Name)));

            return response;
        }
    }
}
