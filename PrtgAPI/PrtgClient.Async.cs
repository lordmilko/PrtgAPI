using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
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
        /// <summary>
        /// Asynchronously calcualte the total number of objects of a given type present on a PRTG Server.
        /// </summary>
        /// <param name="content">The type of object to total.</param>
        /// <returns>The total number of objects of a given type.</returns>
        public async Task<int> GetTotalObjectsAsync(Content content)
        {
            var parameters = new Parameters.Parameters()
            {
                [Parameter.Count] = 0,
                [Parameter.Content] = content
            };

            return Convert.ToInt32((await GetObjectsRawAsync<PrtgObject>(parameters).ConfigureAwait(false)).TotalCount);
        }

        private async Task<List<T>> GetObjectsAsync<T>(Parameters.Parameters parameters)
        {
            return (await GetObjectsRawAsync<T>(parameters).ConfigureAwait(false)).Items;
        }

        private async Task<Data<T>> GetObjectsRawAsync<T>(Parameters.Parameters parameters)
        {
            var response = await ExecuteRequestAsync(XmlFunction.TableData, parameters).ConfigureAwait(false);

            return Data<T>.DeserializeList(response);
        }

        private Data<T> GetObjectsRaw<T>(Parameters.Parameters parameters)
        {
            var response = ExecuteRequest(XmlFunction.TableData, parameters);

            return Data<T>.DeserializeList(response);
        }
    }
}
