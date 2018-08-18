using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using PrtgAPI.Helpers;
using PrtgAPI.Linq;
using PrtgAPI.Linq.Expressions;
using PrtgAPI.Parameters;
using PrtgAPI.Request.Serialization;

namespace PrtgAPI.Request
{
    /// <summary>
    /// Handles constructing requests for retrieving objects and deserializing their responses
    /// </summary>
    class ObjectEngine
    {
        private PrtgClient prtgClient;
        private readonly RequestEngine requestEngine;

        /// <summary>
        /// The maximum number of objects that may be requested in parallel before objects should be requested
        /// serially to prevent web request timeouts.
        /// </summary>
        internal const int SerialStreamThreshold = 20000;

        public ObjectEngine(PrtgClient client, RequestEngine engine)
        {
            prtgClient = client;
            requestEngine = engine;
        }

        #region Get Objects

        internal List<T> GetObjects<T>(IXmlParameters parameters, Action<string> responseValidator = null, bool deserializeAll = true) =>
            GetObjectsRaw<T>(parameters, responseValidator, deserializeAll).Items;

        internal XmlDeserializer<T> GetObjectsRaw<T>(IXmlParameters parameters, Action<string> responseValidator = null, bool deserializeAll = true)
        {
            var response = requestEngine.ExecuteRequest(parameters, responseValidator);

            return SetVersion(XmlDeserializer<T>.DeserializeList(response, deserializeAll));
        }

        internal async Task<List<T>> GetObjectsAsync<T>(IXmlParameters parameters, Action<string> responseValidator = null, bool deserializeAll = true) =>
            (await GetObjectsRawAsync<T>(parameters, responseValidator, deserializeAll).ConfigureAwait(false)).Items;

        internal async Task<XmlDeserializer<T>> GetObjectsRawAsync<T>(IXmlParameters parameters, Action<string> responseValidator = null, bool deserializeAll = true)
        {
            var response = await requestEngine.ExecuteRequestAsync(parameters, responseValidator).ConfigureAwait(false);

            return SetVersion(XmlDeserializer<T>.DeserializeList(response, deserializeAll));
        }

        internal T GetObject<T>(IXmlParameters parameters, Action<string> responseValidator = null)
        {
            var response = requestEngine.ExecuteRequest(parameters, responseValidator);

            return XmlDeserializer<T>.DeserializeType(response);
        }

        internal async Task<T> GetObjectAsync<T>(IXmlParameters parameters)
        {
            var response = await requestEngine.ExecuteRequestAsync(parameters).ConfigureAwait(false);

            return XmlDeserializer<T>.DeserializeType(response);
        }

        internal T GetObject<T>(IJsonParameters parameters, Func<HttpResponseMessage, string> responseParser = null)
        {
            var response = requestEngine.ExecuteRequest(parameters, responseParser);

            var data = JsonDeserializer<T>.DeserializeType(response);

            return data;
        }

        internal async Task<T> GetObjectAsync<T>(IJsonParameters parameters, Func<HttpResponseMessage, Task<string>> responseParser = null)
        {
            var response = await requestEngine.ExecuteRequestAsync(parameters, responseParser).ConfigureAwait(false);

            var data = JsonDeserializer<T>.DeserializeType(response);

            return data;
        }

        private XmlDeserializer<T> SetVersion<T>(XmlDeserializer<T> data)
        {
            if (prtgClient.version == null)
                prtgClient.version = data.Version != null ? Version.Parse(data.Version.Trim('+')) : null;

            return data;
        }

        #endregion
        #region Get Objects XML

        internal XDocument GetObjectsXml(IXmlParameters parameters, Action<string> responseValidator = null) =>
            requestEngine.ExecuteRequest(parameters, responseValidator);

        internal async Task<XDocument> GetObjectsXmlAsync(IXmlParameters parameters, Action<string> responseValidator = null) =>
            await requestEngine.ExecuteRequestAsync(parameters, responseValidator).ConfigureAwait(false);

        #endregion
        #region Stream Objects

        internal IEnumerable<TObject> StreamObjects<TObject, TParam>(TParam parameters, bool serial, bool deserializeAll = true)
            where TObject : IObject
            where TParam : ContentParameters<TObject>, IShallowCloneable<TParam>
        {
            return StreamObjects<TObject, TParam>(
                parameters,                       //Parameters to use for the request
                serial,                           //Whether to stream serial or parallel
                () => prtgClient.GetTotalObjects( //A function used to retrieve the total number of objects that can be retrieved
                    parameters.Content,
                    (parameters[Parameter.FilterXyz] as IEnumerable)?.Cast<SearchFilter>().ToArray()
                ),
                null,                             //The function used to retrieve objects synchronously
                null,                             //The function used to retrieve objects asynchronously
                deserializeAll                    //Whether to deserialize all properties onto the output object
            );
        }

        internal IEnumerable<TObject> StreamObjects<TObject, TParam>(TParam parameters, bool serial, Func<int> getCount,
            Func<TParam, Task<List<TObject>>> getObjectsAsync = null,
            Func<TParam, Tuple<List<TObject>, int>> getObjects = null,
            bool deserializeAll = true)
            where TParam : PageableParameters, IShallowCloneable<TParam>, IXmlParameters
        {
            prtgClient.Log("Preparing to stream objects", LogLevel.Trace);
            prtgClient.Log("Requesting total number of objects", LogLevel.Trace);

            var manager = new StreamManager<TObject, TParam>(
                this,       //Object Engine
                parameters, //Parameters
                getCount,   //Get Count
                serial,     //Serial
                false,      //Direct Call
                getObjects, //Get Objects,
                getObjectsAsync, //Get Objects Async
                deserializeAll //Deserialize All
            );

            if (manager.TotalToRetrieve > SerialStreamThreshold || serial)
            {
                if (manager.TotalToRetrieve > SerialStreamThreshold)
                    prtgClient.Log($"Switching to serial stream mode as over {SerialStreamThreshold} objects were detected", LogLevel.Trace);

                return SerialStreamObjectsInternal(manager);
            }

            return StreamObjectsInternal(manager);
        }

        internal IEnumerable<TObject> StreamObjectsInternal<TObject, TParam>(StreamManager<TObject, TParam> manager)
            where TParam : PageableParameters, IXmlParameters
        {
            Debug.WriteLine("Streaming objects in parallel");

            if (manager.DirectCall)
                prtgClient.Log("Preparing to stream objects", LogLevel.Trace);

            var tasks = new List<Task<List<TObject>>>();

            manager.InitializeRequest();

            for(; manager.RequestIndex < manager.EndIndex;)
            {
                tasks.Add(manager.GetObjectsAsync());

                manager.UpdateRequestIndex();
            }

            var result = new ParallelObjectGenerator<List<TObject>>(tasks.WhenAnyForAll()).SelectMany(m => m);

            return result;
        }

        internal IEnumerable<TObject> SerialStreamObjectsInternal<TObject, TParam>(StreamManager<TObject, TParam> manager)
            where TParam : PageableParameters, IXmlParameters
        {
            Debug.WriteLine("Streaming objects serially");

            if (manager.DirectCall)
                prtgClient.Log("Preparing to serially stream objects", LogLevel.Trace);

            manager.InitializeRequest();

            do
            {
                var response = manager.GetObjects();

                manager.UpdateTotals(response.Item2);

                if (manager.StreamEnded(response.Item1))
                {
                    prtgClient.Log($"No records were returned. Stream ended at request for record {manager.RequestIndex}/{manager.EndIndex}", LogLevel.Trace);
                    break;
                }

                foreach (var obj in response.Item1)
                    yield return obj;

                manager.UpdateRequestIndex();
            } while (manager.RequestIndex < manager.EndIndex);
        }

        #endregion
        #region QueryObjects

        internal IQueryable<TObject> QueryObjects<TObject, TParam>(Expression<Func<TObject, bool>> predicate, bool strict, Func<TParam> parameters, QueryHelper<TObject> queryHelper = null)
            where TObject : ITableObject, IObject
            where TParam : TableParameters<TObject>, IShallowCloneable<TParam>
        {
            var query = new Query<TObject>(
                new TableQueryProvider<TObject, TParam>(
                    parameters,
                    strict,
                    queryHelper,
                    p => StreamObjects<TObject, TParam>(p, true, false)
                )
            );

            if (predicate != null)
                return query.Where(predicate);

            return query;
        }

        #endregion
    }
}
