using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class ReadOnlyResponse : IWebResponse
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
            response = Regex.Replace(response, ObjectSettings.basicMatchRegex, string.Empty, RegexOptions.Singleline);
            response = Regex.Replace(response, ObjectSettings.backwardsMatchRegex, string.Empty, RegexOptions.Singleline);
            response = Regex.Replace(response, ObjectSettings.textAreaRegex, string.Empty, RegexOptions.Singleline);
            response = Regex.Replace(response, ObjectSettings.dropDownListRegex, string.Empty, RegexOptions.Singleline);
            response = Regex.Replace(response, ObjectSettings.dependencyDiv, string.Empty, RegexOptions.Singleline);

            var xml = ObjectSettings.GetXml(new Request.PrtgResponse(response));

            var descendents = xml.Descendants().ToList();

            Assert.AreEqual(0, descendents.Count, string.Join(", ", descendents.Select(d => d.Name)));

            return response;
        }
    }
}
