using System.Net;

namespace PrtgAPI.Tests.UnitTests
{
    interface IWebStatusResponse : IWebResponse
    {
        HttpStatusCode StatusCode { get; set; }
    }
}
