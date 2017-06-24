using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.Helpers;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;
using PrtgAPI.Tests.UnitTests.ObjectTests.Items;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.Responses
{
    public class MultiTypeResponse : IWebResponse
    {
        public string GetResponseText(string address)
        {
            return GetResponse(address).GetResponseText(address);
        }

        private IWebResponse GetResponse(string address)
        {
            var a = GetContent(address);

            switch (GetContent(address))
            {
                case "sensors":   return new SensorResponse(new[] { new SensorItem() });
                case "devices":   return new DeviceResponse(new[] { new DeviceItem() });
                case "groups":    return new GroupResponse(new[] { new GroupItem() });
                case "probenode": return new ProbeResponse(new[] { new ProbeItem() });
                default:
                    throw new NotImplementedException("Unknown content");
            }
        }

        private string GetContent(string address)
        {
            var components = UrlHelpers.CrackUrl(address);

            if (components["content"] != null)
                return components["content"];

            throw new NotImplementedException("unknown address");
        }

        public Task<string> GetResponseTextStream(string address)
        {
            throw new NotImplementedException();
        }

        public HttpStatusCode StatusCode { get; set; }
    }
}
