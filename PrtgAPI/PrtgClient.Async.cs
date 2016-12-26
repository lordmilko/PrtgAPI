using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using PrtgAPI.Helpers;
using PrtgAPI.Objects.Shared;
using PrtgAPI.Parameters;
using PrtgAPI.Objects.Deserialization;

namespace PrtgAPI
{
    public partial class PrtgClient
    {
        public IEnumerable<Sensor> GetSensorsAsync()
        {
            //need to test asking for one page at a time manually and see if we always get 100 if we ask for 100 even if we're on the last page

            var totalSensors = GetTotalObjects(Content.Sensors);

            var tasks = new List<Task<List<Sensor>>>();

            var parameters = new SensorParameters {Count = 500};

            for (int i = 0; i < totalSensors;)
            {
                tasks.Add(GetSensorsAsync(parameters));

                i = i + parameters.Count;
                parameters.Page++;

                if (totalSensors - i < parameters.Count)
                    parameters.Count = totalSensors - i;
            }

            var result = new ParallelObjectGenerator<List<Sensor>>(tasks.WhenAnyForAll()).SelectMany(m => m);

            return result;
        }

        public async Task<int> GetTotalObjectsAsync(Content content)
        {
            var parameters = new Parameters.Parameters()
            {
                [Parameter.Count] = 0,
                [Parameter.Content] = content
            };

            return Convert.ToInt32((await GetObjectsRawAsync<PrtgObject>(parameters)).TotalCount);
        }

        public int GetTotalObjects(Content content)
        {
            var parameters = new Parameters.Parameters()
            {
                [Parameter.Count] = 0,
                [Parameter.Content] = content
            };

            return Convert.ToInt32(GetObjectsRaw<PrtgObject>(parameters).TotalCount);
        }

        public async Task<List<Sensor>> GetSensorsAsync(SensorParameters parameters)
        {
            return await GetObjectsAsync<Sensor>(parameters);
        }

        private async Task<List<T>> GetObjectsAsync<T>(Parameters.Parameters parameters)
        {
            return (await GetObjectsRawAsync<T>(parameters)).Items;
        }

        private async Task<Data<T>> GetObjectsRawAsync<T>(Parameters.Parameters parameters)
        {
            var response = await ExecuteRequestAsync(XmlFunction.TableData, parameters);

            return Data<T>.DeserializeList(response);
        }

        private Data<T> GetObjectsRaw<T>(Parameters.Parameters parameters)
        {
            var response = ExecuteRequest(XmlFunction.TableData, parameters);

            return Data<T>.DeserializeList(response);
        }

        private async Task<XDocument> ExecuteRequestAsync(XmlFunction function, Parameters.Parameters parameters)
        {
            var url = new PrtgUrl(Server, Username, PassHash, function, parameters);

            var response = await ExecuteRequestAsync(url);

            return XDocument.Parse(XDocumentHelpers.SanitizeXml(response));
        }

        private async Task<string> ExecuteRequestAsync(PrtgUrl url)
        {
            string response;

            try
            {
                using (var client = new System.Net.WebClient()) //TODO - make our iwebclient implement idisposable?
                    //of course, now that our client STORES its webclient now, that means WE have to
                    //implement idisposable?
                {
                    response = await client.DownloadStringTaskAsync(url.Url);
                }
            }
            catch (WebException ex)
            {
                if (ex.Response == null)
                    throw;

                var webResponse = (HttpWebResponse) ex.Response;

                if (webResponse.StatusCode == HttpStatusCode.BadRequest)
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream(), System.Text.Encoding.UTF8))
                    {
                        response = reader.ReadToEnd();

                        var xDoc = XDocument.Parse(response);
                        var errorMessage = xDoc.Descendants("error").First().Value;

                        throw new PrtgRequestException($"PRTG was unable to complete the request. The server responded with the following error: {errorMessage}", ex);
                    }
                }

                throw;
            }

            return response;
        }
    }
}
