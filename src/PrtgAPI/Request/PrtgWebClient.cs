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
        private CookieContainer cookies = new CookieContainer();

        private string server;

        public PrtgWebClient(bool ignoreSSL, string server)
        {
            //PrtgWebClient will be initialized before PrtgClient has validated server value
            if (server != null)
            {
                server = server.ToLower();

                //Strip the protocol and port (if applicable)
                this.server = Regex.Replace(server, "(.+?://)?(.+?)(:.*)?", "$2");
            }

            if (ignoreSSL)
                ServicePointManager.ServerCertificateValidationCallback += IgnoreSSLCallback;

            handler.CookieContainer = cookies;

            //.NET Core should use DefaultProxyCredentials
            var proxy = WebRequest.DefaultWebProxy;

            if (proxy != null && proxy.Credentials == null)
                proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
            
            asyncClient = new HttpClient(handler);
        }

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
            return asyncClient.GetAsync(request.Url, token);
        }
    }
}