using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.Helpers;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestItems;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses
{
    public class CloneResponse : MultiTypeResponse
    {
        private string responseAddress;

        public CloneResponse()
        {
            responseAddress = "https://prtg.example.com/public/login.htm?loginurl=/object.htm?id=9999&errormsg=";
        }

        public CloneResponse(string address)
        {
            responseAddress = address;
        }

        protected override IWebResponse GetResponse(ref string address, string function)
        {
            switch (function)
            {
                case nameof(XmlFunction.TableData):
                    return GetTableResponse(address);
                case nameof(CommandFunction.DuplicateObject):
                    address = responseAddress;
                    return new BasicResponse(string.Empty);
                default:
                    throw GetUnknownFunctionException(function);
            }
        }

        private IWebResponse GetTableResponse(string address)
        {
            var content = GetContent(address);

            if (content == Content.Sensors)
                return new SensorResponse();
            else
                throw new NotSupportedException($"Content type {content} is not supported by {nameof(CloneResponse)}");
        }
    }
}
