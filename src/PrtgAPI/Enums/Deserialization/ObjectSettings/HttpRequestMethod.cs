using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies request methods to use for HTTP requests.
    /// </summary>
    public enum HttpRequestMethod
    {
        /// <summary>
        /// Execute the request using GET.
        /// </summary>
        [XmlEnum("GET")]
        GET,

        /// <summary>
        /// Execute the request using POST.
        /// </summary>
        [XmlEnum("POST")]
        POST,

        /// <summary>
        /// Execute the request using HEAD. HEAD is similar to GET, however only includes the response headers, omitting the response body.
        /// </summary>
        [XmlEnum("HEAD")]
        HEAD
    }
}
