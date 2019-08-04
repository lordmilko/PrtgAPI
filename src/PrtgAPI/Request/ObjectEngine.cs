using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using PrtgAPI.Linq;
using PrtgAPI.Linq.Expressions;
using PrtgAPI.Parameters;
using PrtgAPI.Request.Serialization;
using PrtgAPI.Utilities;

namespace PrtgAPI.Request
{
#pragma warning disable 618
    /// <summary>
    /// Handles constructing requests for retrieving objects and deserializing their responses
    /// </summary>
    class ObjectEngine
    {
        private PrtgClient prtgClient;
        private readonly RequestEngine requestEngine;
        internal XmlEngine XmlEngine { get; }

        /// <summary>
        /// The maximum number of objects that may be requested in parallel before objects should be requested
        /// serially to prevent web request timeouts.
        /// </summary>
        internal const int SerialStreamThreshold = 20000;

        public ObjectEngine(PrtgClient client, RequestEngine engine, IXmlSerializer xmlSerializer)
        {
            prtgClient = client;
            requestEngine = engine;
            XmlEngine = new XmlEngine(xmlSerializer);
        }

        #region Get Objects

        internal List<T> GetObjects<T>(IXmlParameters parameters, Action<PrtgResponse> responseValidator = null, bool validateValueTypes = true, CancellationToken token = default(CancellationToken)) =>
            GetObjectsRaw<T>(parameters, responseValidator, null, validateValueTypes, token).Items;

        internal TableData<T> GetObjectsRaw<T>(
            IXmlParameters parameters,
            Action<PrtgResponse> responseValidator = null,
            Func<HttpResponseMessage, PrtgResponse> responseParser = null,
            bool validateValueTypes = true,
            CancellationToken token = default(CancellationToken))
        {
            return ParseInvalidXml(() =>
            {
                using (var response = requestEngine.ExecuteRequest(parameters, responseValidator, responseParser, token: token))
                {
                    return SetVersion(XmlEngine.DeserializeTable<T>(response, validateValueTypes));
                }
            });
        }

        internal async Task<List<T>> GetObjectsAsync<T>(
            IXmlParameters parameters,
            Action<PrtgResponse> responseValidator = null,
            Func<HttpResponseMessage, Task<PrtgResponse>> responseParser = null,
            bool validateValueTypes = true,
            CancellationToken token = default(CancellationToken)) =>
            (await GetObjectsRawAsync<T>(parameters, responseValidator, responseParser, validateValueTypes, token).ConfigureAwait(false)).Items;

        internal async Task<TableData<T>> GetObjectsRawAsync<T>(
            IXmlParameters parameters,
            Action<PrtgResponse> responseValidator = null,
            Func<HttpResponseMessage, Task<PrtgResponse>> responseParser = null,
            bool validateValueTypes = true,
            CancellationToken token = default(CancellationToken))
        {
            return await ParseInvalidXmlAsync(async () =>
            {
                using (var response = await requestEngine.ExecuteRequestAsync(parameters, responseValidator, responseParser, token: token).ConfigureAwait(false))
                {
                    return SetVersion(XmlEngine.DeserializeTable<T>(response, validateValueTypes));
                }
            }).ConfigureAwait(false);
        }

        internal T GetObject<T>(IXmlParameters parameters, Action<PrtgResponse> responseValidator = null)
        {
            return ParseInvalidXml(() =>
            {
                using (var response = requestEngine.ExecuteRequest(parameters, responseValidator))
                {
                    return XmlEngine.DeserializeObject<T>(response);
                }
            });
        }

        internal async Task<T> GetObjectAsync<T>(IXmlParameters parameters, CancellationToken token = default(CancellationToken))
        {
            return await ParseInvalidXmlAsync(async () =>
            {
                using (var response = await requestEngine.ExecuteRequestAsync(parameters, token: token).ConfigureAwait(false))
                {
                    return XmlEngine.DeserializeObject<T>(response);
                }
            }).ConfigureAwait(false);
        }

        internal T GetObject<T>(IJsonParameters parameters, Func<HttpResponseMessage, PrtgResponse> responseParser = null, CancellationToken token = default(CancellationToken))
        {
            using (var response = requestEngine.ExecuteRequest(parameters, responseParser, token))
            {
                var data = JsonDeserializer<T>.DeserializeType(response);

                return data;
            }
        }

        internal async Task<T> GetObjectAsync<T>(IJsonParameters parameters, Func<HttpResponseMessage, Task<PrtgResponse>> responseParser = null, CancellationToken token = default(CancellationToken))
        {
            using (var response = await requestEngine.ExecuteRequestAsync(parameters, responseParser, token).ConfigureAwait(false))
            {
                var data = JsonDeserializer<T>.DeserializeType(response);

                return data;
            }
        }

        private TableData<T> SetVersion<T>(TableData<T> data)
        {
            if (prtgClient.version == null)
                prtgClient.version = data.Version != null ? Version.Parse(data.Version.Trim('+')) : null;

            return data;
        }

        #endregion
        #region Get Objects XML

        internal XDocument GetObjectsXml(IXmlParameters parameters, Action<PrtgResponse> responseValidator = null, Func<HttpResponseMessage, PrtgResponse> responseParser = null, CancellationToken token = default(CancellationToken))
        {
            return ParseInvalidXml(() =>
            {
                using (var response = requestEngine.ExecuteRequest(parameters, responseValidator, responseParser, token))
                {
                    return XDocument.Load(response);
                }
            });
        }

        internal async Task<XDocument> GetObjectsXmlAsync(IXmlParameters parameters, Action<PrtgResponse> responseValidator = null, Func<HttpResponseMessage, Task<PrtgResponse>> responseParser = null, CancellationToken token = default(CancellationToken))
        {
            return await ParseInvalidXmlAsync(async () =>
            {
                using (var response = await requestEngine.ExecuteRequestAsync(parameters, responseValidator, responseParser, token).ConfigureAwait(false))
                {
                    return XDocument.Load(response);
                }
            }).ConfigureAwait(false);
        }

        #endregion
        #region Stream Objects

        internal IEnumerable<TObject> StreamObjects<TObject, TParam>(TParam parameters, bool serial, bool validateValueTypes = true)
            where TObject : IObject
            where TParam : ContentParameters<TObject>, IShallowCloneable<TParam>
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters), "Parameters cannot be null.");

            return StreamObjects<TObject, TParam>(
                parameters,                       //Parameters to use for the request
                serial,                           //Whether to stream serial or parallel
                () => prtgClient.GetTotalObjects( //A function used to retrieve the total number of objects that can be retrieved
                    parameters.Content,
                    (parameters[Parameter.FilterXyz] as IEnumerable)?.Cast<SearchFilter>().ToArray()
                ),
                null,                             //The function used to retrieve objects synchronously
                null,                             //The function used to retrieve objects asynchronously
                validateValueTypes                //Whether to deserialize all properties onto the output object
            );
        }

        internal IEnumerable<TObject> StreamObjects<TObject, TParam>(TParam parameters, bool serial, Func<int> getCount,
            Func<TParam, Task<List<TObject>>> getObjectsAsync = null,
            Func<TParam, Tuple<List<TObject>, int>> getObjects = null,
            bool validateValueTypes = true)
            where TParam : PageableParameters, IShallowCloneable<TParam>, IXmlParameters
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters), "Parameters cannot be null.");

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
                validateValueTypes //Validate Value Types
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

        T ParseInvalidXml<T>(Func<T> action)
        {
            //Sometimes PRTG may return invalid characters in their responses (e.g. \0). Assume the request is valid by default

            try
            {
                return action();
            }
            catch(XmlException ex)
            {
                if (requestEngine.IsDirty)
                    throw;

                prtgClient.Log($"XmlSerializer encountered exception '{ex.Message}' while processing request. Retrying request and flagging engine as dirty.", LogLevel.Trace);

                requestEngine.IsDirty = true;
                return action();
            }
        }

        async Task<T> ParseInvalidXmlAsync<T>(Func<Task<T>> action)
        {
            //Sometimes PRTG may return invalid characters in their responses (e.g. \0). Assume the request is valid by default

            try
            {
                return await action().ConfigureAwait(false);
            }
            catch (XmlException ex)
            {
                if (requestEngine.IsDirty)
                    throw;

                prtgClient.Log($"XmlSerializer encountered exception '{ex.Message}' while processing request. Retrying request and flagging engine as dirty.", LogLevel.Trace);

                requestEngine.IsDirty = true;
                return await action().ConfigureAwait(false);
            }
        }
    }
#pragma warning restore 618
}
