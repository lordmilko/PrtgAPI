using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.Helpers;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.Responses
{
    public class CloneResponse : MultiTypeResponse
    {
        protected override IWebResponse GetResponse(ref string address)
        {
            var function = GetFunction(address);

            switch (function)
            {
                case nameof(XmlFunction.TableData):
                    return GetTableResponse(address);
                case nameof(CommandFunction.DuplicateObject):
                    address = "https://prtg.example.com/public/login.htm?loginurl=/object.htm?id=9999&errormsg=";
                    return new BasicResponse(string.Empty);
                default:
                    throw new NotImplementedException($"Unknown function '{function}' passed to {nameof(CloneResponse)}");
            }
        }

        private IWebResponse GetTableResponse(string address)
        {
            var components = UrlHelpers.CrackUrl(address);

            Content content = components["content"].ToEnum<Content>();

            if (content == Content.Sensors)
                return new SensorResponse(new Items.SensorItem[] {});
            else
                throw new NotSupportedException($"Content type {content} is not supported by {nameof(CloneResponse)}");
        }
    }
}
