using System;
using System.Linq;
using PrtgAPI.Utilities;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    public class RestartProbeResponse : MultiTypeResponse
    {
        private ProbeItem[] probes;

        private int logRequestCount;

        protected override IWebResponse GetResponse(ref string address, string function)
        {
            switch (function)
            {
                case nameof(JsonFunction.GetStatus):
                    return new ServerStatusResponse(new ServerStatusItem());
                case nameof(XmlFunction.TableData):
                    return GetTableResponse(address);
                case nameof(CommandFunction.RestartProbes):
                    return new BasicResponse(string.Empty);
                default:
                    throw GetUnknownFunctionException(function);
            }
        }

        private IWebResponse GetTableResponse(string address)
        {
            var components = UrlUtilities.CrackUrl(address);

            Content content = components["content"].DescriptionToEnum<Content>();

            switch (content)
            {
                case Content.Probes:
                    return GetProbeResponse();
                case Content.Logs:
                    return GetMessageResponse();
                default:
                    throw new NotImplementedException($"Unknown content '{content}' requested from {nameof(RestartProbeResponse)}");
            }
        }

        private IWebResponse GetProbeResponse()
        {
            probes = Enumerable.Range(0, 2).Select(i => new ProbeItem(name: $"127.0.0.1{i}", objid: (1000 + i).ToString())).ToArray();

            return new ProbeResponse(probes);
        }

        private IWebResponse GetMessageResponse()
        {
            logRequestCount++;

            MessageResponse response;

            //Disconnect both probes, then reconnect them again

            switch (logRequestCount)
            {
                case 1:
                    response = new MessageResponse();
                    break;

                case 2:
                    response = new MessageResponse(new MessageItem(objid: probes[0].ObjId, statusRaw: "613"));
                    break;

                case 3:
                    response = new MessageResponse(new MessageItem(objid: probes[1].ObjId, statusRaw: "613"));
                    break;

                case 4:
                    response = new MessageResponse(new MessageItem(objid: probes[0].ObjId, statusRaw: "612"));
                    break;

                case 5:
                    response = new MessageResponse(new MessageItem(objid: probes[1].ObjId, statusRaw: "612"));
                    break;

                default:
                    throw new NotImplementedException($"Handler missing for request #{logRequestCount}");
            }

            return response;
        }
    }
}
