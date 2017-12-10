using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.RegularExpressions;
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

        #region JSON + Response Parser

        internal string ExecuteRequest(JsonFunction function, Parameters.Parameters parameters, Func<HttpResponseMessage, string> responseParser = null)
        {
            var url = GetPrtgUrl(function, parameters);

            var response = ExecuteRequest(url, responseParser);

            return response;
        }

        internal async Task<string> ExecuteRequestAsync(JsonFunction function, Parameters.Parameters parameters, Func<HttpResponseMessage, Task<string>> responseParser = null)
        {
            var url = GetPrtgUrl(function, parameters);

            var response = await ExecuteRequestAsync(url, responseParser).ConfigureAwait(false);

            return response;
        }

        #endregion
        #region XML + Response Validator

        internal XDocument ExecuteRequest(XmlFunction function, Parameters.Parameters parameters, Action<string> responseValidator = null)
        {
            var url = GetPrtgUrl(function, parameters);

            var response = ExecuteRequest(url);

            responseValidator?.Invoke(response);

            return XDocument.Parse(XDocumentHelpers.SanitizeXml(response));
        }

        internal async Task<XDocument> ExecuteRequestAsync(XmlFunction function, Parameters.Parameters parameters, Action<string> responseValidator = null)
        {
            var url = GetPrtgUrl(function, parameters);

            var response = await ExecuteRequestAsync(url).ConfigureAwait(false);

            responseValidator?.Invoke(response);

            return XDocument.Parse(XDocumentHelpers.SanitizeXml(response));
        }

        #endregion
        #region Command + Response Parser

        internal string ExecuteRequest(CommandFunction function, Parameters.Parameters parameters, Func<HttpResponseMessage, string> responseParser = null)
        {
            var url = GetPrtgUrl(function, parameters);

            var response = ExecuteRequest(url, responseParser);

            return response;
        }

        internal async Task<string> ExecuteRequestAsync(CommandFunction function, Parameters.Parameters parameters, Func<HttpResponseMessage, Task<string>> responseParser = null)
        {
            var url = GetPrtgUrl(function, parameters);

            var response = await ExecuteRequestAsync(url, responseParser).ConfigureAwait(false);

            return response;
        }

        #endregion
        #region HTML

        internal string ExecuteRequest(HtmlFunction function, Parameters.Parameters parameters)
        {
            var url = GetPrtgUrl(function, parameters);

            var response = ExecuteRequest(url);

            return response;
        }

        internal async Task<string> ExecuteRequestAsync(HtmlFunction function, Parameters.Parameters parameters)
        {
            var url = GetPrtgUrl(function, parameters);

            var response = await ExecuteRequestAsync(url);

            return response;
        }

        #endregion

        private string ExecuteRequest(PrtgUrl url, Func<HttpResponseMessage, string> responseParser = null)
        {
            prtgClient.Log($"Synchronously executing request {url.Url}");

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
                    var innerException = ex.InnerException;

                    if (ex.InnerException is TaskCanceledException)
                        innerException = new TimeoutException($"The server timed out while executing request.", ex.InnerException);

                    var result = HandleRequestException(ex.InnerException?.InnerException, innerException, url, ref retriesRemaining, () =>
                    {
                        if (innerException != null)
                            throw innerException;
                    });

                    if (!result)
                        throw;
                }
            } while (true);
        }

        private async Task<string> ExecuteRequestAsync(PrtgUrl url, Func<HttpResponseMessage, Task<string>> responseParser = null)
        {
            prtgClient.Log($"Asynchronously executing request {url.Url}");

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
                catch (HttpRequestException ex)
                {
                    var inner = ex.InnerException as WebException;

                    var result = HandleRequestException(inner, ex, url, ref retriesRemaining, null);

                    if (!result)
                        throw;
                }
                catch (TaskCanceledException ex)
                {
                    throw new TimeoutException($"The server timed out while executing request.", ex);
                }
            } while (true);
        }

        private void HandleSocketException(Exception ex, string url)
        {
            var socketException = ex?.InnerException as SocketException;

            if (socketException != null)
            {
                var port = socketException.Message.Substring(socketException.Message.LastIndexOf(':') + 1);

                var protocol = url.Substring(0, url.IndexOf(':')).ToUpper();

                if (socketException.SocketErrorCode == SocketError.TimedOut)
                {
                    throw new TimeoutException($"Connection timed out while communicating with remote server via {protocol} on port {port}. Confirm server address and port are valid and PRTG Service is running", ex);
                }
                else if (socketException.SocketErrorCode == SocketError.ConnectionRefused)
                {
                    throw new WebException($"Server rejected {protocol} connection on port {port}. Please confirm expected server protocol and port, PRTG Core Service is running and that any SSL certificate is trusted", ex);
                }
            }
        }

        private bool HandleRequestException(Exception innerMostEx, Exception fallbackHandlerEx, PrtgUrl url, ref int retriesRemaining, Action thrower)
        {
            if (innerMostEx != null) //Synchronous + Asynchronous
            {
                if (retriesRemaining > 0)
                    prtgClient.HandleEvent(prtgClient.retryRequest, new RetryRequestEventArgs(innerMostEx, url.Url, retriesRemaining));
                else
                {
                    HandleSocketException(innerMostEx, url.Url);

                    throw innerMostEx;
                }
            }
            else
            {
                if (fallbackHandlerEx != null && retriesRemaining > 0)
                    prtgClient.HandleEvent(prtgClient.retryRequest, new RetryRequestEventArgs(fallbackHandlerEx, url.Url, retriesRemaining));
                else
                {
                    thrower?.Invoke();

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

                errorMsg = errorMsg.Replace("<br/><ul><li>", " ").Replace("</li></ul><br/>", " ");

                throw new PrtgRequestException($"PRTG was unable to complete the request. The server responded with the following error: {errorMsg}");
            }

            if (responseText.StartsWith("<div class=\"errormsg\">")) //Example: GetProbeProperties specifying a content type of Probe instead of ProbeNode
            {
                var msgEnd = responseText.IndexOf("</h3>");

                var substr = responseText.Substring(0, msgEnd);

                var substr1 = Regex.Replace(substr, "\\.</.+?><.+?>", ". ");

                var regex = new Regex("<.+?>");
                var newStr = regex.Replace(substr1, "");

                throw new PrtgRequestException($"PRTG was unable to complete the request. The server responded with the following error: {newStr}");
            }
        }

        private PrtgUrl GetPrtgUrl(CommandFunction function, Parameters.Parameters parameters) =>
            new PrtgUrl(prtgClient.connectionDetails, function, parameters);

        private PrtgUrl GetPrtgUrl(HtmlFunction function, Parameters.Parameters parameters) =>
            new PrtgUrl(prtgClient.connectionDetails, function, parameters);

        private PrtgUrl GetPrtgUrl(JsonFunction function, Parameters.Parameters parameters) =>
            new PrtgUrl(prtgClient.connectionDetails, function, parameters);

        private PrtgUrl GetPrtgUrl(XmlFunction function, Parameters.Parameters parameters) =>
            new PrtgUrl(prtgClient.connectionDetails, function, parameters);
    }
}
