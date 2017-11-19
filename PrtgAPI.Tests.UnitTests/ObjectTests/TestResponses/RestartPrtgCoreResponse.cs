using System;
using System.Net;
using System.Net.Http;
using PrtgAPI.PowerShell.Cmdlets;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses
{
    public class RestartPrtgCoreResponse : MultiTypeResponse
    {
        private RestartPrtgCore.RestartStage restartStage = RestartPrtgCore.RestartStage.Shutdown;

        private int shutdownRequestCount;

        protected override IWebResponse GetResponse(ref string address, string function)
        {
            switch (function)
            {
                case nameof(CommandFunction.RestartServer):
                    return new BasicResponse(string.Empty);
                default:
                    return GetRestartServerProgressResponse();
            }
        }

        private IWebResponse GetRestartServerProgressResponse()
        {
            switch (restartStage)
            {
                case RestartPrtgCore.RestartStage.Shutdown:
                    if (shutdownRequestCount == 0)
                    {
                        shutdownRequestCount++;
                        return new MessageResponse();
                    }
                    restartStage = RestartPrtgCore.RestartStage.Restart;
                    throw new WebException();

                case RestartPrtgCore.RestartStage.Restart:
                    restartStage = RestartPrtgCore.RestartStage.Startup;
                    throw new HttpRequestException();

                case RestartPrtgCore.RestartStage.Startup:
                    return new MessageResponse(new TestItems.MessageItem(statusRaw: "1"));

                default:
                    throw new NotImplementedException($"Unknown stage '{restartStage}'");
            }
        }
    }
}
