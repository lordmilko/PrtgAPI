using System.Collections.Generic;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Helpers;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    public class LogAddressValidatorResponse : MultiTypeResponse
    {
        private string str;

        public LogAddressValidatorResponse(string str)
        {
            this.str = str;
        }

        public LogAddressValidatorResponse(string str, Dictionary<Content, int> countOverride) : base(countOverride)
        {
            this.str = str;
        }

        protected override IWebResponse GetResponse(ref string address, string function)
        {
            if (function == nameof(XmlFunction.TableData))
            {
                var components = UrlHelpers.CrackUrl(address);

                Content content = components["content"].DescriptionToEnum<Content>();

                if (content == Content.Logs)
                {
                    if (components["columns"] != "objid,name")
                    {
                        components.Remove("content");
                        components.Remove("columns");
                        components.Remove("username");
                        components.Remove("passhash");

                        if (components["start"] != null)
                            components.Remove("start");

                        var filtered = HttpUtility.UrlDecode(components.ToString());

                        if (filtered != str)
                            Assert.Fail($"Address was '{filtered}' instead of '{str}'");
                    }
                }
            }

            return base.GetResponse(ref address, function);
        }
    }
}
