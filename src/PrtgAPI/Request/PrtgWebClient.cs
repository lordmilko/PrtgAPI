using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PrtgAPI.Request
{
    [ExcludeFromCodeCoverage]
    internal class PrtgWebClient : IWebClient
    {
        private WebClient syncClient = new WebClient();
        private HttpClient asyncClient = new HttpClient();

        private HttpClientHandler handler = new HttpClientHandler();

        private string server;

        public PrtgWebClient(bool ignoreSSL, string server)
        {
            //PrtgWebClient will be initialized before PrtgClient has validated server value
            if (server != null)
            {
                server = server.ToLower();

                //Strip the protocol and port (if applicable)
                this.server = Regex.Replace(server, "(.+?://)?(.+?)(:.*)?(/.*)?", "$2");
            }

            //Bypass SSL validation errors if requested
            if (ignoreSSL)
            {
#if NETFRAMEWORK
                ServicePointManager.ServerCertificateValidationCallback += IgnoreSSLCallback;
#else
                handler.ServerCertificateCustomValidationCallback = IgnoreSSLCallback;
#endif
            }

            //Configure any proxies to use default network credentials for automatic
            //authentication
            var proxy = WebRequest.DefaultWebProxy;

            if (proxy != null && proxy.Credentials == null)
            {
#if NETFRAMEWORK
                proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
#else
                handler.DefaultProxyCredentials = CredentialCache.DefaultNetworkCredentials;
#endif
            }

            //.NET Framework 4.6.1 does not include TLS 1.2 by default, and future
            //versions may eventually decide to deprecate it. To ensure backwards
            //compatibility with all versions of PRTG, do not remove items from this list
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            asyncClient = new HttpClient(handler);
        }

#if NETFRAMEWORK
        private bool IgnoreSSLCallback(object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            //Default validation scheme is called before our custom handler is executed. If SSL
            //contained no errors, allow request immediately
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            var request = sender as HttpWebRequest;

            if (request != null)
            {
                //Otherwise, was the request executed against our PRTG server?
                if (request.Address.Host.ToLower() == server)
                    return true;
            }

            return false;
        }
#else
        private bool IgnoreSSLCallback(HttpRequestMessage request, X509Certificate2 certificate, X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            if (request.RequestUri.Host.ToLower() == server)
                return true;

            return false;
        }
#endif

        public Task<HttpResponseMessage> SendSync(PrtgRequestMessage request, CancellationToken token)
        {
            return SendAsync(request, token);
        }

        public Task<HttpResponseMessage> SendAsync(PrtgRequestMessage request, CancellationToken token)
        {
            //Cannot use HttpCompletionOption.ResponseHeadersRead without manually disposing
            //HttpResponseMessage objects, which will also dispose internal stream. Existing code
            //needs to be refactored to have the deserialization happen in a callback to ExecuteRequest
            //so that there is no problem wrapping the HttpResponseMessage up in a using for both
            //sync/async

            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = request.Uri
            };

            foreach (var header in request.Headers)
                httpRequestMessage.Headers.Add(header.Key, header.Value);

            return asyncClient.SendAsync(httpRequestMessage, token);
        }
    }
}
