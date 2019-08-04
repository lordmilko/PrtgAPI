using System;
using System.Net.Http.Headers;

namespace PrtgAPI.Tests.UnitTests
{
    interface IWebContentHeaderResponse : IWebResponse
    {
        /// <summary>
        /// Specifies an action used to modify the default Content Headers of a response.
        /// </summary>
        Action<HttpContentHeaders> HeaderAction { get; set; }
    }
}
