using System;
using System.Net;
using System.Net.Http;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    public class RestartPrtgCoreResponse : MultiTypeResponse
    {
        private RestartCoreStage restartCoreStage = RestartCoreStage.Shutdown;

        private int shutdownRequestCount;

        protected override IWebResponse GetResponse(ref string address, string function)
        {
            switch (function)
            {
                case nameof(JsonFunction.GetStatus):
                    return new ServerStatusResponse(new ServerStatusItem());
                case nameof(CommandFunction.RestartServer):
                    return new BasicResponse(string.Empty);
                default:
                    return GetRestartServerProgressResponse();
            }
        }

        private IWebResponse GetRestartServerProgressResponse()
        {
            switch (restartCoreStage)
            {
                case RestartCoreStage.Shutdown:
                    if (shutdownRequestCount == 0)
                    {
                        shutdownRequestCount++;
                        return new MessageResponse();
                    }
                    restartCoreStage = RestartCoreStage.Restart;
                    throw new WebException();

                case RestartCoreStage.Restart:
                    restartCoreStage = RestartCoreStage.Startup;
                    throw new HttpRequestException();

                case RestartCoreStage.Startup:
                    return new MessageResponse(new TestItems.MessageItem(statusRaw: "1"));

                default:
                    throw new NotImplementedException($"Unknown stage '{restartCoreStage}'");
            }
        }
    }
}
