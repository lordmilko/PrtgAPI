using System.Net;

namespace PrtgAPI.Tests.UnitTests.InfrastructureTests.Support
{
    interface IWebStatusResponse : IWebResponse
    {
        HttpStatusCode StatusCode { get; set; }
    }
}
