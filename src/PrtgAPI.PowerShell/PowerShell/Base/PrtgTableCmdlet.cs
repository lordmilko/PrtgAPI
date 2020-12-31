using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PrtgAPI.Attributes;
using PrtgAPI.Linq;
using PrtgAPI.Parameters;
using PrtgAPI.Reflection;
using PrtgAPI.Reflection.Cache;
using PrtgAPI.Utilities;
using IDynamicParameters = System.Management.Automation.IDynamicParameters;

namespace PrtgAPI.PowerShell.Base
{
    /// <summary>
    /// Base class for all cmdlets that return advanced objects found in tables (sensors, devices, probes, logs, etc).
    /// </summary>
    /// <typeparam name="TObject">The type of objects that will be retrieved.</typeparam>
    /// <typeparam name="TParam">The type of parameters to use to retrieve objects</typeparam>
    public abstract class PrtgTableCmdlet<TObject, TParam> : PrtgObjectCmdlet<TObject>, IStreamableCmdlet<PrtgTableCmdlet<TObject, TParam>, TObject, TParam>
        where TObject : ITableObject, IObject
        where TParam : TableParameters<TObject>
    {
        /// <summary>
        /// <para type="description">Filter the response to objects with a certain name. Can include wildcards.</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 0, HelpMessage = "Filter the response to objects with a certain name. Can include wildcards.")]
        public string[] Name { get; set; }

        /// <summary>
        /// <para type="description">Filter the response to objects that match one or more criteria.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true, Position = 1, HelpMessage = "Filter the response to objects that match one or more criteria.")]
        public SearchFilter[] Filter { get; set; }

        /// <summary>
        /// <para type="description">Maximum number of results to return. Note: when this parameter is specified wildcard filters
        /// such as <see cref="Name"/> may behave unexpectedly when wildcard characters are not used and records are being filtered
        /// by an additional property other than <see cref="Property.ParentId"/>.</para>
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Maxmimum number of results to return.")]
        public int? Count { get; set; }

        /// <summary>
        /// The type of content this cmdlet will retrieve.
        /// </summary>
        protected Content content;

        /// <summary>
        /// The search filters that were specified via dynamic parameters.
        /// </summary>
        private List<SearchFilter> dynamicParameters;
        private PropertyDynamicParameterSet<Property> dynamicParameterSet;

        private List<SearchFilter> filters;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrtgTableCmdlet{TObject,TParam}"/> class. 
        /// </summary>
        /// <param name="content">The type of content this cmdlet will retrieve.</param>
        /// <param name="shouldStream">Whether this cmdlet should have streaming enabled.</param>
        protected PrtgTableCmdlet(Content content, bool? shouldStream)
        {
            ((IStreamableCmdlet<PrtgTableCmdlet<TObject, TParam>, TObject, TParam>)this).StreamProvider = new StreamableCmdletProvider<PrtgTableCmdlet<TObject, TParam>, TObject, TParam>(this, shouldStream);
            this.content = content;
        }

        /// <summary>
        /// Provides an enhanced one-time, preprocessing functionality for the cmdlet.
        /// </summary>
        protected override void BeginProcessingEx()
        {
            if (this is IDynamicParameters && dynamicParameterSet != null)
            {
                dynamicParameters = dynamicParameterSet.GetBoundParameters(this, (p, v) =>
                {
                    var cleaned = PSObjectUtilities.CleanPSObject(v);

                    if (cleaned == null)
                        return new List<SearchFilter>();

                    var underlying = cleaned.GetType().GetElementType() ?? cleaned.GetType();

                    if (underlying == typeof(object))
                    {
                        if (cleaned.IsIEnumerable())
                        {
                            var first = cleaned.ToIEnumerable().FirstOrDefault();

                            if (first != null)
                                underlying = first.GetType();
                        }
                    }

                    if (underlying == typeof(string))
                        return GetWildcardFilters(p, cleaned, val => val.ToString());
                    if (typeof(IStringEnum).IsAssignableFrom(underlying))
                        return GetWildcardFilters(p, cleaned, val => ((IStringEnum)val).StringValue);

                    return new[] {GetPipelineFilter(p, cleaned)};
                });
            }

            base.BeginProcessingEx();
        }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            var parameters = ProcessParameters();
            var records = GetRecordsInternal(parameters);

            WriteList(records);
        }

        #region Core

        private IEnumerable<TObject> GetRecordsInternal(TParam parameters)
        {
            IEnumerable<TObject> records;

            if (ProgressManager.GetRecordsWithVariableProgress)
                records = GetResultsWithVariableProgress(() => GetObjects(parameters));
            else if (ProgressManager.GetResultsWithProgress)
                records = GetResultsWithProgress(() => GetObjects(parameters));
            else
            {
                if (StreamProvider.StreamResults || StreamProvider.ForceStream)
                    records = StreamProvider.StreamResultsWithProgress(parameters, Count, () => GetObjects(parameters));
                else
                    records = GetObjects(parameters);
            }

            records = PostProcessRecords(records);
            records = SortReturnedRecordsRunner(records);

            return records;
        }

        /// <summary>
        /// Retrieves all records of a specified type from a PRTG Server using the types default parameters.
        /// This method should never be executed.
        /// </summary>
        /// <returns>The records retrieved from the PRTG Server.</returns>
        [ExcludeFromCodeCoverage]
        protected override IEnumerable<TObject> GetRecords()
        {
            return GetObjects(CreateParameters());
        }

        private IEnumerable<TObject> GetObjects(TParam parameters)
        {
            if (StreamProvider.StreamResults || StreamProvider.ForceStream)
                return GetObjectsWhenStreaming(parameters);

            return GetObjectsWhenNotStreaming(parameters);
        }

        private IEnumerable<TObject> GetObjectsWhenStreaming(TParam parameters)
        {
            if (ProgressManager.WatchStream)
            {
                if (typeof(TObject) == typeof(Log))
                {
                    return (IEnumerable<TObject>)new InfiniteLogGenerator(
                        client.GetLogs,
                        (LogParameters)(object)parameters,
                        ((IWatchableCmdlet)this).Interval,
                        i =>
                        {
                            if (Stopping)
                                throw new PipelineStoppedException();

                            return true;
                        },
                        CancellationToken,
                        logs => (IEnumerable<Log>)PostProcessRecords((IEnumerable<TObject>)logs)
                    );
                }

                throw new NotImplementedException($"Don't know how to watch objects of type ({typeof(TObject).Name}).");
            }

            if (Count != null && filters != null)
            {
                int previousCount;
                Func<int> getCount;

                if (StreamProvider.StreamCount != null)
                {
                    previousCount = StreamProvider.StreamCount.Value;
                    getCount = () => previousCount;
                }
                else
                    getCount = () => ((IStreamableCmdlet<PrtgTableCmdlet<TObject, TParam>, TObject, TParam>)this).GetStreamTotalObjects(parameters);

                StreamProvider.SetStreamCount(parameters, Count);

                var iterator = new TakeIterator<TObject, TParam>(
                    StreamProvider.StreamCount.Value,
                    parameters,
                    (p, c) => StreamProvider.StreamRecords<TObject>(p, Count, c),
                    getCount,
                    PostProcessRecords,
                    SortReturnedRecordsRunner
                );

                return iterator;
            }

            return StreamProvider.StreamRecords<TObject>(parameters, Count);
        }

        private IEnumerable<TObject> GetObjectsWhenNotStreaming(TParam parameters)
        {
            if (Count != null && filters != null)
                return StreamObjectsWhenNotStreaming(parameters);

            //Not streaming and don't need to. Just execute a normal request
            var objs = GetObjectsAndAdditionalRecords(parameters);

            return objs;
        }

        private IEnumerable<TObject> StreamObjectsWhenNotStreaming(TParam parameters)
        {
            var streamCount = StreamCount();

            //If we're piping from groups, don't specify a count so that we may retrieve all records and retrieve the
            //correct amount that match the criteria
            if (!streamCount)
                parameters.Count = null;

            var iterator = new TakeIterator<TObject, TParam>(
                Count.Value,
                parameters,
                GetNotStreamingStreamer(streamCount),
                () => StreamProvider.GetTotalExist(parameters),
                PostProcessRecords,
                SortReturnedRecordsRunner,
                streamCount
            );

            return iterator;
        }

        private Func<TParam, Func<int>, IEnumerable<TObject>> GetNotStreamingStreamer(bool streamCount)
        {
            Func<TParam, Func<int>, IEnumerable<TObject>> streamer;

            if (streamCount)
            {
                //If streamCount == true (e.g. we just did Get-Sensor ping -count 2) we need to keep
                //track of our Start offset as we keep trying to find records; as such, use the regular stream method
                streamer = (p, c) => StreamProvider.StreamRecords<TObject>(p, Count, c);
            }
            else
            {
                //If streamCount == false, we are recursing from groups. We want to get ALL CHILDREN
                //and then filter for the specified count CLIENT SIDE. As such, there is no fiddling with Start offsets,
                //and the fact getCount was not passed to GetObjectsAndAdditionalRecords doesn't matter.
                streamer = (p, c) => GetObjectsAndAdditionalRecords(p);
            }

            return streamer;
        }

        internal virtual List<TObject> GetObjectsInternal(TParam parameters) =>
            client.ObjectEngine.GetObjects<TObject>(parameters);

        internal virtual bool StreamCount()
        {
            return false;
        }

        /// <summary>
        /// Retrieves additional records not included in the initial request.
        /// </summary>
        /// <param name="parameters">The parameters that were used to perform the initial request.</param>
        protected virtual IEnumerable<TObject> GetAdditionalRecords(TParam parameters) => new List<TObject>();

        private IEnumerable<TObject> GetObjectsAndAdditionalRecords(TParam parameters)
        {
            var objs = GetObjectsInternal(parameters);

            return objs.Union(GetAdditionalRecords(parameters));
        }

        internal object GetDynamicParameters(params string[] parameterSets)
        {
            if (parameterSets.Length == 0)
                parameterSets = new[] { ParameterSet.Dynamic };

            if (dynamicParameterSet == null)
            {
                var properties = ReflectionCacheManager.Get(typeof(TObject)).Properties.
                    Where(p => p.GetAttribute<PropertyParameterAttribute>() != null).
                    Select(p => Tuple.Create((Property)p.GetAttribute<PropertyParameterAttribute>().Property, p)).ToList();

                dynamicParameterSet = new PropertyDynamicParameterSet<Property>(
                    parameterSets,
                    e => ReflectionCacheManager.GetArrayPropertyType(properties.FirstOrDefault(p => p.Item1 == e)?.Item2.Property.PropertyType),
                    this
                );
            }

            return dynamicParameterSet.Parameters;
        }

        private void PreProcessFilter()
        {
            filters = filters.Select(PreProcessFilterInternal).ToList();
        }

        private SearchFilter PreProcessFilterInternal(SearchFilter filter)
        {
            var typeProperties = typeof(TObject).GetTypeCache().Properties;

            //Filter value could in fact be an enum type. Lookup the property from the current cmdlet's type
            var property = typeProperties.FirstOrDefault(p => p.GetAttributes<PropertyParameterAttribute>().Any(a => a.Property.Equals(filter.Property)));

            if (property == null)
                return filter;

            var cleanValue = Regex.Replace(filter.Value.ToString(), "[^a-zA-Z0-9_]", string.Empty);

            var result = ParseEnumFilter(property.Property, filter, cleanValue);

            if (result != null)
                return result;

            //Filter value could in fact be the display value of a raw type. See whether any internal properties starting with
            //the expected property name exist also ending with "raw". If so, if that property is an enum, strip all
            //invalid characters from the value and try and lookup an enum member with that name
            var propertyRaw = typeProperties.Where(p => p.Property.GetSetMethod() == null).FirstOrDefault(p => p.Property.Name.ToLower() == (property.Property.Name + "raw").ToLower());

            if (propertyRaw == null)
                return filter;

            result = ParseEnumFilter(propertyRaw.Property, filter, cleanValue);

            if (result != null)
                return result;

            if (filter.Property == Property.Type)
            {
                SensorType type;

                if (Enum.TryParse(filter.Value.ToString(), true, out type))
                    return new SearchFilter(filter.Property, filter.Operator, type);
            }

            return filter;
        }

        private SearchFilter ParseEnumFilter(PropertyInfo property, SearchFilter filter, string cleanValue)
        {
            var type = property.PropertyType.GetUnderlyingType();

            if (type.IsEnum)
            {
                if (Enum.GetNames(type).Any(e => e.ToLower() == cleanValue.ToLower()))
                {
                    var enumValue = Enum.Parse(type, cleanValue, true);

                    return new SearchFilter(filter.Property, filter.Operator, enumValue);
                }
            }

            return null;
        }

        #endregion
        #region Progress

        /// <summary>
        /// Display the initial progress message for the first cmdlet in a chain.<para/>
        /// If the cmdlet is streaming results, will display "detecting total number of items" and return the total number of items that will be retrieved.
        /// </summary>
        /// <returns>If the cmdlet is streaming, the total number of objects that will be retrieved. Otherwise, -1.</returns>
        protected override int DisplayFirstInChainMessage()
        {
            if (StreamProvider.StreamResults)
            {
                ProgressManager.WriteProgress($"PRTG {IObjectExtensions.GetTypeDescription(typeof(TObject))} Search", "Detecting total number of items");
                StreamProvider.StreamCount = client.GetTotalObjects(content);

                return StreamProvider.StreamCount.Value;
            }

            return base.DisplayFirstInChainMessage();
        }

        /// <summary>
        /// Retrieves the number of elements returned from a request that may or may not have been streamed.
        /// </summary>
        /// <param name="records">The records to count. This collection will only be enumerated if results were not retrieved via streaming (indicating the collection is not yet complete)</param>
        /// <param name="count">If results were streamed, the total number of objects initially reported by the server. Otherwise, will be-1.</param>
        /// <returns>The number of elements returned or to be returned from the request.</returns>
        protected override IEnumerable<TObject> GetCount(IEnumerable<TObject> records, ref int count)
        {
            if (StreamProvider.StreamResults)
                return records;
            else
                return base.GetCount(PostProcessRecords(records), ref count);
        }

        #endregion
        #region Pre-Process

        private TParam ProcessParameters()
        {
            //Clear out any filters that may have been set for the previous pipeline object
            filters = null;

            if (Filter != null && Filter.Length > 0)
            {
                foreach (var f in Filter)
                    AddToFilter(f);
            }

            var parameters = CreateParameters();

            ProcessNameFilter();
            ProcessAdditionalParameters();
            ProcessDynamicParameters();
            ValidateFilters();

            if (filters != null)
            {
                PreProcessFilter();
                parameters.SearchFilters = filters;

                StreamProvider.StreamResults = false;
            }

            if (Count != null)
            {
                //Count is ignored to an extent due to the use of the TakeIterator.
                //However, as recurse cmdlets do not stream, specifying a Count is useful in
                //limiting the number of records returned per child level. e.g. if we specified
                //-Count 6, if we ask for 6 from everyone we'll eventually get the total amount
                //we need.
                parameters.Count = Count.Value;
                StreamProvider.StreamResults = false;
            }

            if (StreamProvider.StreamResults && ProgressManager.PartOfChain && !ProgressManager.FirstInChain)
                StreamProvider.StreamResults = false;

            return parameters;
        }

        private void ProcessNameFilter()
        {
            ProcessWildcardArrayFilter(Property.Name, Name);
        }

        private void ProcessDynamicParameters()
        {
            if (dynamicParameters != null)
            {
                foreach (var filter in dynamicParameters)
                    AddToFilter(filter);
            }
        }

        /// <summary>
        /// Processes additional parameters specific to the current cmdlet.
        /// </summary>
        protected virtual void ProcessAdditionalParameters()
        {
        }

        internal virtual void ValidateFilters()
        {
            if (filters != null)
            {
                var safeProperties = new[]
                {
                    Property.ParentId
                };

                var fs = filters.Where(f => safeProperties.All(p => p != f.Property)).ToList();

                if (fs.Count == 1 && fs.Single().Property == Property.Name && Name != null)
                {
                    if (!Name.Any(n => n.Contains("*")))
                        fs.Single().Operator = FilterOperator.Equals;
                }
            }
        }

        /// <summary>
        /// Add an array of filter values that may contain wildcard values.
        /// </summary>
        /// <param name="property">The property to filter for.</param>
        /// <param name="arr">The array of wildcards.</param>
        protected void ProcessWildcardArrayFilter(Property property, string[] arr)
        {
            if (arr != null)
            {
                foreach (var value in arr)
                {
                    AddWildcardFilter(property, value);
                }
            }
        }

        /// <summary>
        /// Add a filter for the value contained in a <see cref="NameOrObject{T}"/> object.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="PrtgObject"/> possibly contained in <paramref name="obj"/>.</typeparam>
        /// <param name="objectProperty">The property to filter on if <paramref name="obj"/> contains an object.</param>
        /// <param name="obj">The object that either contains a <see cref="PrtgObject"/> or a wildcard expression specifying the object name to filter by.</param>
        /// <param name="getValue">A function that retrieves the property to filter by from the <see cref="PrtgObject"/> when <paramref name="obj"/> contains an object.</param>
        /// <param name="nameProperty">The property to filter by when <paramref name="obj"/> does not contain an object. If this value is null, <paramref name="objectProperty"/> will be used as the filter property.</param>
        protected void AddNameOrObjectFilter<T>(Property objectProperty, NameOrObject<T> obj, Func<T, object> getValue, Property? nameProperty = null) where T : PrtgObject
        {
            if (obj.IsObject)
                AddPipelineFilter(objectProperty, getValue(obj.Object));
            else
                AddWildcardFilter(nameProperty ?? objectProperty, obj.Name);
        }

        /// <summary>
        /// Adds a filter for a concrete value that came in from the pipeline (such as an Id)
        /// </summary>
        /// <param name="property">The property to filter on.</param>
        /// <param name="value">The value to filter for.</param>
        /// <param name="invalidatesStream">Whether adding this filter precludes the current cmdlet from streaming.</param>
        protected void AddPipelineFilter(Property property, object value, bool invalidatesStream = true)
        {
            AddToFilter(GetPipelineFilter(property, value), invalidatesStream);
        }

        /// <summary>
        /// Add a filter for a value that may contain wildcard characters.
        /// </summary>
        /// <param name="property">The property to filter on.</param>
        /// <param name="value">The value to filter for.</param>
        protected void AddWildcardFilter(Property property, string value)
        {
            AddToFilter(GetWildcardFilter(property, value));
        }

        private SearchFilter GetWildcardFilter(Property property, string value)
        {
            var parts = CleanWildcard(value);

            var trimmed = string.Join(",", parts);

            //If another filter has been specified, an equals filter will become case sensitive. To work around this, we always do "contains", and then filter for
            //what we really wanted once the response is returned
            var filter = new SearchFilter(property, FilterOperator.Contains, trimmed);

            return filter;
        }

        private IEnumerable<SearchFilter> GetWildcardFilters(Property property, object value, Func<object, string> getString)
        {
            if (value is IEnumerable && !(value is string))
            {
                foreach (var v in (IEnumerable)value)
                {
                    yield return GetWildcardFilter(property, getString(v));
                }
            }
            else
                yield return GetWildcardFilter(property, getString(value));
        }

        private SearchFilter GetPipelineFilter(Property property, object value) =>
            new SearchFilter(property, FilterOperator.Equals, value);

        private void AddToFilter(SearchFilter filter, bool invalidatesStream = true)
        {
            if (invalidatesStream)
                StreamProvider.StreamResults = false;

            if (filters == null)
                filters = new List<SearchFilter>();

            filters.Add(filter);
        }

        internal List<string> CleanWildcard(string str)
        {
            return str.Split('*').Where(p => p != string.Empty).ToList();
        }

        /// <summary>
        /// Creates a new parameter object capable of being passed to <see cref="GetObjects(TParam)"/> 
        /// </summary>
        /// <returns>The default set of parameters.</returns>
        protected abstract TParam CreateParameters();

        #endregion
        #region Post-Process

        private IEnumerable<TObject> PostProcessRecords(IEnumerable<TObject> records)
        {
            records = FilterResponseRecordsByName(records);

            records = FilterResponseRecordsByDynamic(records);

            records = PostProcessAdditionalFilters(records);

            return records;
        }

        private IEnumerable<TObject> FilterResponseRecordsByName(IEnumerable<TObject> records)
        {
            return FilterResponseRecordsByWildcardArray(Name, s => ((ITableObject)s).Name, records);
        }

        private IEnumerable<TObject> FilterResponseRecordsByDynamic(IEnumerable<TObject> records)
        {
            if (dynamicParameters != null)
            {
                //Create a list of mappings between each Property and its associated value
                var parameters = dynamicParameterSet.GetBoundParameters(this, Tuple.Create).Where(IsStringLike).ToList();

                foreach (var filter in parameters)
                    records = FilterResponseRecordsByDynamicInternal(filter, records);
            }

            return records;
        }

        private bool IsStringLike(Tuple<Property, object> val)
        {
            if (val.Item2 == null)
                return false;

            Type underlying = val.Item2.GetType();

            if (underlying.IsArray)
                underlying = underlying.GetElementType();

            return underlying == typeof(string) || typeof(IStringEnum).IsAssignableFrom(underlying);
        }

        private IEnumerable<TObject> FilterResponseRecordsByDynamicInternal(Tuple<Property, object> filter, IEnumerable<TObject> records)
        {
            //Get the PropertyInfo the filter Property corresponds to.
            var property = ReflectionCacheManager.Get(typeof(TObject))
                .Properties
                .First(
                    p => p.GetAttribute<PropertyParameterAttribute>()?.Property.Equals(filter.Item1) == true
                );

            if (property.Property.PropertyType.IsArray)
                throw new NotImplementedException("Cannot filter array properties dynamically.");

            //Was the value that was specified enumerable? The answer should always be yes, as
            //DynamicParameterPropertyTypes only defines array types
            if (filter.Item2.IsIEnumerable())
            {
                //Get the values that were specified
                var values = GetDynamicWildcardFilterValues(filter);

                return FilterResponseRecordsByWildcardArray(values, r =>
                {
                    if (typeof(IStringEnum).IsAssignableFrom(property.Property.PropertyType))
                    {
                        return ((IStringEnum) property.Property.GetValue(r)).StringValue;
                    }

                    return property.Property.GetValue(r).ToString();
                }, records);
            }

            throw new NotImplementedException($"All PropertyInfo values should be array types, however type {filter.Item2.GetType().FullName} was not.");
        }

        private string[] GetDynamicWildcardFilterValues(Tuple<Property, object> filter)
        {
            var values = filter.Item2.ToIEnumerable().Select(v =>
            {
                if (v is IStringEnum)
                {
                    var str = ((IStringEnum)v).StringValue;

                    if (!str.Contains("*"))
                    {
                        var originalFilter = new SearchFilter(filter.Item1, str);

                        var newFilter = PreProcessFilterInternal(originalFilter);

                        if (newFilter.Value != originalFilter.Value)
                        {
                            return ((Enum)newFilter.Value).EnumToXml();
                        }
                    }

                    return str;
                }

                return v.ToString();
            }).ToArray();

            return values;
        }

        /// <summary>
        /// Process any post retrieval filters specific to the current cmdlet.
        /// </summary>
        /// <param name="records">The records to filter.</param>
        /// <returns>The filtered records.</returns>
        protected virtual IEnumerable<TObject> PostProcessAdditionalFilters(IEnumerable<TObject> records)
        {
            return records;
        }

        /// <summary>
        /// Filter a response with a wildcard expression contained in a <see cref="NameOrObject{T}"/> value.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="PrtgObject"/> possibly contained in <paramref name="obj"/>.</typeparam>
        /// <param name="obj">The object that possibly contains a wildcard expression.</param>
        /// <param name="getProperty">A function that retrieves the property to filter by from each record.</param>
        /// <param name="records">The records to filter.</param>
        /// <returns>If <paramref name="obj"/> contains a wildcard expression, the filtered collection. Otherwise, the original collection.</returns>
        protected IEnumerable<TObject> FilterResponseRecordsByNameOrObjectName<T>(NameOrObject<T> obj, Func<TObject, string> getProperty, IEnumerable<TObject> records) where T : PrtgObject
        {
            if (obj != null && !obj.IsObject)
                return FilterResponseRecords(records, obj.Name, getProperty);

            return records;
        }

        private IEnumerable<TObject> SortReturnedRecordsRunner(IEnumerable<TObject> records)
        {
            if (!StreamProvider.StreamResults && !StreamProvider.ForceStream)
                return SortReturnedRecords(records);

            return records;
        }

        /// <summary>
        /// Specifies how the records returned from this cmdlet should be sorted. By default, no sorting is performed.
        /// </summary>
        /// <param name="records">The records to sort.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> representing the sorted collection.</returns>
        protected virtual IEnumerable<TObject> SortReturnedRecords(IEnumerable<TObject> records)
        {
            return records;
        }

        #endregion       
        #region IStreamableCmdlet

        Tuple<List<TObject>, int> IStreamableCmdlet<PrtgTableCmdlet<TObject, TParam>, TObject, TParam>.GetStreamObjects(
            TParam parameters)
        {
            var raw = client.ObjectEngine.GetObjectsRaw<TObject>(parameters);

            return Tuple.Create(raw.Items, raw.TotalCount);
        }

        [ExcludeFromCodeCoverage]
        async Task<List<TObject>> IStreamableCmdlet<PrtgTableCmdlet<TObject, TParam>, TObject, TParam>.GetStreamObjectsAsync(TParam parameters) =>
            await client.ObjectEngine.GetObjectsAsync<TObject>(parameters).ConfigureAwait(false);

        int IStreamableCmdlet<PrtgTableCmdlet<TObject, TParam>, TObject, TParam>.GetStreamTotalObjects(TParam parameters) =>
            client.GetTotalObjects(content, parameters.SearchFilters?.ToArray());

        StreamableCmdletProvider<PrtgTableCmdlet<TObject, TParam>, TObject, TParam> IStreamableCmdlet<PrtgTableCmdlet<TObject, TParam>, TObject, TParam>.StreamProvider { get; set; }

        internal StreamableCmdletProvider<PrtgTableCmdlet<TObject, TParam>, TObject, TParam> StreamProvider => (
            (IStreamableCmdlet<PrtgTableCmdlet<TObject, TParam>, TObject, TParam>)this).StreamProvider;

        #endregion
    }
}
