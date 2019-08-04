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
                return "<div class=\"errormsg\"><p>PRTG Network Monitor has discovered a problem. Your last request could not be processed properly.</p><h3>Error message: Sorry, a read-only user account is not allowed to access this web page.</h3><small style=\"padding:5px;text-align:left\">Url: /controls/addsensor2.htm<br>Params: id=2055&sensortype=exexml&username=prtguser&passhash=***&</small></div>";

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
