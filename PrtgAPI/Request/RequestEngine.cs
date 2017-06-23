using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using PrtgAPI.Helpers;

namespace PrtgAPI.Request
{
    class RequestEngine
    {
        private IWebClient webClient;
        private PrtgClient prtgClient;

        internal RequestEngine(PrtgClient prtgClient, IWebClient webClient)
        {
            this.prtgClient = prtgClient;
            this.webClient = webClient;
        }

        internal string ExecuteRequest(JsonFunction function, Parameters.Parameters parameters)
        {
            var url = new PrtgUrl(prtgClient.Server, prtgClient.UserName, prtgClient.PassHash, function, parameters);

            var response = ExecuteRequest(url);

            return response;
        }

        internal XDocument ExecuteRequest(XmlFunction function, Parameters.Parameters parameters)
        {
            var url = new PrtgUrl(prtgClient.Server, prtgClient.UserName, prtgClient.PassHash, function, parameters);

            var response = ExecuteRequest(url);

            return XDocument.Parse(XDocumentHelpers.SanitizeXml(response));
        }

        internal async Task<XDocument> ExecuteRequestAsync(XmlFunction function, Parameters.Parameters parameters)
        {
            var url = new PrtgUrl(prtgClient.Server, prtgClient.UserName, prtgClient.PassHash, function, parameters);

            var response = await ExecuteRequestAsync(url).ConfigureAwait(false);

            return XDocument.Parse(XDocumentHelpers.SanitizeXml(response));
        }

        internal string ExecuteRequest(CommandFunction function, Parameters.Parameters parameters, Func<HttpResponseMessage, string> responseParser = null)
        {
            var url = new PrtgUrl(prtgClient.Server, prtgClient.UserName, prtgClient.PassHash, function, parameters);

            var response = ExecuteRequest(url, responseParser);

            return response;
        }

        internal async Task ExecuteRequestAsync(CommandFunction function, Parameters.Parameters parameters)
        {
            var url = new PrtgUrl(prtgClient.Server, prtgClient.UserName, prtgClient.PassHash, function, parameters);

            var response = await ExecuteRequestAsync(url).ConfigureAwait(false);
        }

        internal string ExecuteRequest(HtmlFunction function, Parameters.Parameters parameters)
        {
            var url = new PrtgUrl(prtgClient.Server, prtgClient.UserName, prtgClient.PassHash, function, parameters);

            var response = ExecuteRequest(url);

            return response;
        }

        internal async Task<string> ExecuteRequestAsync(HtmlFunction function, Parameters.Parameters parameters)
        {
            var url = new PrtgUrl(prtgClient.Server, prtgClient.UserName, prtgClient.PassHash, function, parameters);

            var response = await ExecuteRequestAsync(url);

            return response;
        }

        private string ExecuteRequest(PrtgUrl url, Func<HttpResponseMessage, string> responseParser = null)
        {
            prtgClient.Log($"Synchronously executing request '{url.Url}'");

            int retriesRemaining = prtgClient.RetryCount;

            do
            {
                try
                {
                    var response = webClient.GetSync(url.Url).Result;

                    string responseText = null;

                    if (responseParser != null)
                        responseText = responseParser(response);

                    if (responseText == null)
                        responseText = response.Content.ReadAsStringAsync().Result;

                    ValidateHttpResponse(response, responseText);

                    return responseText;
                }
                catch (Exception ex) when (ex is AggregateException || ex is TaskCanceledException || ex is HttpRequestException)
                {
                    var result = HandleRequestException(ex.InnerException?.InnerException, ex.InnerException, url, ref retriesRemaining, () =>
                    {
                        if (ex.InnerException != null)
                            throw ex.InnerException;
                    });

                    if (!result)
                        throw;
                }
            } while (true);
        }

        private async Task<string> ExecuteRequestAsync(PrtgUrl url, Func<HttpResponseMessage, Task<string>> responseParser = null)
        {
            prtgClient.Log($"Asynchronously executing request '{url.Url}'");

            int retriesRemaining = prtgClient.RetryCount;

            do
            {
                try
                {
                    var response = await webClient.GetAsync(url.Url).ConfigureAwait(false);

                    string responseText = null;

                    if (responseParser != null)
                        responseText = await responseParser(response).ConfigureAwait(false);

                    if (responseText == null)
                        responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    ValidateHttpResponse(response, responseText);

                    return responseText;
                }
                catch (HttpRequestException ex) //todo: test making invalid requests
                {
                    var inner = ex.InnerException as WebException;

                    var result = HandleRequestException(inner, ex, url, ref retriesRemaining, null);

                    if (!result)
                        throw;
                }
            } while (true);
        }

        private bool HandleRequestException(Exception innerMostEx, Exception fallbackHandlerEx, PrtgUrl url, ref int retriesRemaining, Action thrower)
        {
            if (innerMostEx != null)
            {
                if (retriesRemaining > 0)
                    prtgClient.HandleEvent(prtgClient.retryRequest, new RetryRequestEventArgs(innerMostEx, url.Url, retriesRemaining));
                else
                    throw innerMostEx;
            }
            else
            {
                if (fallbackHandlerEx != null && retriesRemaining > 0)
                    prtgClient.HandleEvent(prtgClient.retryRequest, new RetryRequestEventArgs(fallbackHandlerEx, url.Url, retriesRemaining));
                else
                {
                    thrower();

                    return false;
                }
            }

            if (retriesRemaining > 0)
            {
                var attemptsMade = prtgClient.RetryCount - retriesRemaining + 1;
                var delay = prtgClient.RetryDelay * attemptsMade;

                Thread.Sleep(delay * 1000);
            }

            retriesRemaining--;

            return true;
        }

        private void ValidateHttpResponse(HttpResponseMessage response, string responseText)
        {
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var xDoc = XDocument.Parse(responseText);
                var errorMessage = xDoc.Descendants("error").First().Value;

                throw new PrtgRequestException($"PRTG was unable to complete the request. The server responded with the following error: {errorMessage}");
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new HttpRequestException("Could not authenticate to PRTG; the specified username and password were invalid.");
            }

            response.EnsureSuccessStatusCode();

            if (response.RequestMessage?.RequestUri?.AbsolutePath == "/error.htm")
            {
                var errorUrl = response.RequestMessage.RequestUri.ToString();

                var queries = UrlHelpers.CrackUrl(errorUrl);
                var errorMsg = queries["errormsg"];

                throw new PrtgRequestException($"PRTG was unable to complete the request. The server responded with the following error: {errorMsg}");
            }
        }
    }
}
