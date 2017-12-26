using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text.RegularExpressions;
using PrtgAPI.Attributes;
using PrtgAPI.Helpers;
using PrtgAPI.Objects.Shared;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Progress;

namespace PrtgAPI.PowerShell.Base
{
    /// <summary>
    /// Base class for all cmdlets that return advanced objects found in tables (sensors, devices, probes, logs, etc)
    /// </summary>
    /// <typeparam name="TObject">The type of objects that will be retrieved.</typeparam>
    /// <typeparam name="TParam">The type of parameters to use to retrieve objects</typeparam>
    public abstract class PrtgTableBaseCmdlet<TObject, TParam> : PrtgObjectCmdlet<TObject> where TParam : TableParameters<TObject> where TObject : ObjectTable
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
        /// <para type="description">Maximum number of results to return.</para>
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Maxmimum number of results to return.")]
        public int? Count { get; set; }

        /// <summary>
        /// The type of content this cmdlet will retrieve.
        /// </summary>
        protected Content content;

        private int? progressThreshold;

        private bool streamResults;
        private bool streamSerial;

        /// <summary>
        /// Indicates that current cmdlet should stream, regardless of whether it determines this is required.
        /// </summary>
        protected bool forceStream;

        private int? streamCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrtgTableBaseCmdlet{TObject,TParam}"/> class. 
        /// </summary>
        /// <param name="content">The type of content this cmdlet will retrieve.</param>
        /// <param name="progressThreshold">The numeric threshold at which this cmdlet should show a progress bar when retrieving results.</param>
        /// <param name="streamSerial">Indicates that if the current cmdlet streams, it should do so one at a time instead of all at once.</param>
        protected PrtgTableBaseCmdlet(Content content, int? progressThreshold, bool streamSerial = false)
        {
            this.content = content;
            this.progressThreshold = progressThreshold;

            if (progressThreshold != null)
            {
                streamResults = true;
                this.streamSerial = streamSerial;
            }
        }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            ValidateParameters();

            var parameters = ProcessParameters();
            var records = GetRecordsInternal(parameters);

            WriteList(records);

            //Clear the filters for the next element on the pipeline, which will simply reuse the existing PrtgTableBaseCmdlet object
            Filter = null;
        }

        private void ValidateParameters()
        {
            if (MyInvocation.BoundParameters.ContainsKey("Id") && MyInvocation.BoundParameters["Id"] == null)
                throw new ParameterBindingException("The -Id parameter was specified however the parameter value was null.");
        }

        private IEnumerable<TObject> GetFilteredObjects(TParam parameters)
        {
            if (Filter != null)
            {
                PreProcessFilter();
                parameters.SearchFilter = Filter;
            }
                
            return GetObjects(parameters);
        }

        private void PreProcessFilter()
        {
            Filter = Filter.Select(filter =>
            {
                //Filter value could in fact be an enum type. Lookup the property from the current cmdlet's type
                var property = typeof (TObject).GetProperties().FirstOrDefault(p => p.GetCustomAttribute<PropertyParameterAttribute>()?.Name == filter.Property.ToString());

                if (property == null)
                    return filter;

                var cleanValue = Regex.Replace(filter.Value.ToString(), "[^a-zA-Z0-9_]", string.Empty);

                var result = ParseEnumFilter(property, filter, cleanValue);

                if (result != null)
                    return result;

                //Filter value could in fact be the display value of a raw type. See whether any internal properties starting with
                //the expected property name exist also ending with "raw". If so, if that property is an enum, strip all
                //invalid characters from the value and try and lookup an enum member with that name
                var propertyRaw = typeof (TObject).GetProperties(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(p => p.Name.ToLower() == (property.Name + "raw").ToLower());

                if (propertyRaw == null)
                    return filter;

                result = ParseEnumFilter(propertyRaw, filter, cleanValue);

                if (result != null)
                    return result;

                return filter;
            }).ToArray();
        }

        private SearchFilter ParseEnumFilter(PropertyInfo property, SearchFilter filter, string cleanValue)
        {
            var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

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

        private IEnumerable<TObject> GetRecordsInternal(TParam parameters)
        {
            IEnumerable<TObject> records;

            if (ProgressManager.GetRecordsWithVariableProgress)
                records = GetResultsWithVariableProgress(() => GetFilteredObjects(parameters)); //todo: need to test this works properly
            else if (ProgressManager.GetResultsWithProgress)
                records = GetResultsWithProgress(() => GetFilteredObjects(parameters));
            else
            {
                if (streamResults || forceStream)
                    records = StreamResultsWithProgress(parameters);
                else
                    records = GetFilteredObjects(parameters);
            }

            records = PostProcessRecords(records);

            return records;
        }

        private IEnumerable<TObject> PostProcessRecords(IEnumerable<TObject> records)
        {
            records = FilterResponseRecordsByName(records);

            records = PostProcessAdditionalFilters(records);

            return records;
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

        private TParam ProcessParameters()
        {
            var parameters = CreateParameters();

            ProcessNameFilter();
            ProcessAdditionalParameters();
            
            if (Filter != null)
                streamResults = false;

            if (Count != null)
            {
                parameters.Count = Count.Value;
                streamResults = false;
            }

            if (streamResults && ProgressManager.PartOfChain && !ProgressManager.FirstInChain)
                streamResults = false;

            return parameters;
        }

        /// <summary>
        /// Processes additional parameters specific to the current cmdlet.
        /// </summary>
        protected virtual void ProcessAdditionalParameters()
        {
        }

        private void ProcessNameFilter()
        {
            if (Name != null)
            {
                foreach (var value in Name)
                {
                    AddWildcardFilter(Property.Name, value);
                }
            }
        }

        private IEnumerable<TObject> StreamResultsWithProgress(TParam parameters)
        {
            ProgressManager.Scenario = ProgressScenario.StreamProgress;

            ProgressManager.WriteProgress($"PRTG {GetTypeDescription(typeof(TObject))} Search", "Detecting total number of items");

            streamCount = client.GetTotalObjects(content);

            //Normally if a filter has been specified PrtgAPI won't stream, and so the custom parameters are not necessary.
            //However, if a cmdlet has specified it wants to force a stream, we apply our filters and use our custom parameters.
            var records = forceStream ? GetFilteredObjects(parameters) : GetRecords();

            if (streamCount > progressThreshold)
            {
                //We'll be replacing this progress record, so just null it out via a call to CompleteProgress()
                //We strategically set the TotalRecords AFTER calling this method, to avoid CompleteProgress truly completing the record
                ProgressManager.CompleteProgress();
                SetObjectSearchProgress(ProcessingOperation.Retrieving);
                ProgressManager.TotalRecords = streamCount;
            }
            else //We won't be showing progress, so complete this record
            {
                ProgressManager.TotalRecords = streamCount;
                ProgressManager.CompleteProgress();
            }

            return records;
        }

        /// <summary>
        /// Display the initial progress message for the first cmdlet in a chain.<para/>
        /// If the cmdlet is streaming results, will display "detecting total number of items" and return the total number of items that will be retrieved.
        /// </summary>
        /// <returns>If the cmdlet is streaming, the total number of objects that will be retrieved. Otherwise, -1.</returns>
        protected override int DisplayFirstInChainMessage()
        {
            if (streamResults)
            {
                ProgressManager.WriteProgress($"PRTG {GetTypeDescription(typeof(TObject))} Search", "Detecting total number of items");
                streamCount = client.GetTotalObjects(content);

                return streamCount.Value;
            }

            return base.DisplayFirstInChainMessage();
        }

        /// <summary>
        /// Retrieves the number of elements returned from a request that may or may not have been streamed.
        /// </summary>
        /// <param name="records">The records to count. This collection will only be enumerated if results were not retrieved via streaming (indicating the collection is not yet complete)</param>
        /// <param name="count">If results were streamed, the total number of objects initially reported by the server. Otherwise, will be-1.</param>
        /// <returns></returns>
        protected override IEnumerable<TObject> GetCount(IEnumerable<TObject> records, ref int count)
        {
            if (streamResults)
                return records;
            else
                return base.GetCount(PostProcessRecords(records), ref count);
        }

        private IEnumerable<TObject> FilterResponseRecordsByName(IEnumerable<TObject> records)
        {
            if (Name != null)
            {
                records = records.Where(record => Name
                    .Select(name => new WildcardPattern(name, WildcardOptions.IgnoreCase))
                    .Any(filter => filter.IsMatch(record.Name))
                );
            }

            return records;
        }

        /// <summary>
        /// Adds a filter for a concrete value that came in from the pipeline (such as an Id)
        /// </summary>
        /// <param name="property">The property to filter on.</param>
        /// <param name="value">The value to filter for.</param>
        /// <param name="invalidatesStream">Whether adding this filter precludes the current cmdlet from streaming.</param>
        protected void AddPipelineFilter(Property property, object value, bool invalidatesStream = true)
        {
            var filter = new SearchFilter(property, FilterOperator.Equals, value);

            AddToFilter(filter, invalidatesStream);
        }

        /// <summary>
        /// Add a filter for a value that may contain wildcard characters.
        /// </summary>
        /// <param name="property">The property to filter on.</param>
        /// <param name="value">The value to filter for.</param>
        protected void AddWildcardFilter(Property property, string value)
        {
            var trimmed = value.Trim('*');

            //If the string contains a * in the middle, split the string into its components,
            //then ask PRTG to return all objects whose name matches the longest section of the string.
            //We will further filter this in FilterResponseRecordsByName
            var parts = value.Split('*').Where(p => p != string.Empty).ToList();

            if (parts.Count > 1)
            {
                var longest = parts.OrderByDescending(p => p.Length).First();

                trimmed = longest;
            }

            //If another filter has been specified, an equals filter will become case sensitive. To work around this, we always do "contains", and then filter for
            //what we really wanted once the response is returned
            var filter = new SearchFilter(property, FilterOperator.Contains, trimmed);

            AddToFilter(filter);
        }

        private void AddToFilter(SearchFilter filter, bool invalidatesStream = true)
        {
            if (invalidatesStream)
                streamResults = false;

            Filter = Filter?.Concat(new[] {filter}).ToArray() ?? new[] {filter};
        }

        /// <summary>
        /// Creates a new parameter object capable of being passed to <see cref="GetObjects(TParam)"/> 
        /// </summary>
        /// <returns>The default set of parameters.</returns>
        protected abstract TParam CreateParameters();

        /// <summary>
        /// Retrieves all records of a specified type from a PRTG Server using the types default parameters.
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<TObject> GetRecords()
        {
            return GetObjects(CreateParameters());
        }

        private IEnumerable<TObject> GetObjects(TParam parameters)
        {
            if (streamResults || forceStream)
            {
                //Depending on the number of items we're streaming, we may have made so many requests to PRTG that it can't possibly
                //respond to any cmdlets further downstream until all of the streaming requests have been completed

                //If we're forcing a stream, we may have actually specified the Count parameter. As such, override the
                //number of results to retrieve
                if (forceStream && Count != null)
                    streamCount = Count.Value;

                //We went down an alternate code path that doesn't retrieve the total number of needed objects
                if (streamCount == null)
                {
                    if (forceStream)
                        streamCount = client.GetTotalObjects(content);
                    else
                        throw new NotImplementedException("Attempted to stream without specifying a stream count. This indicates a bug");
                }

                //As such, if there are no other PRTG cmdlets after us, stream as normal. Otherwise, only request a couple at a time
                //so the PRTG will be able to handle the next cmdlet's request
                if (!streamSerial && ProgressManager.Scenario == ProgressScenario.StreamProgress && !ProgressManager.CacheManager.PipelineRemainingHasCmdlet<PrtgCmdlet>() && ProgressManager.ProgressPipelinesCount == 1) //There are no other cmdlets after us
                    return client.StreamObjectsInternal(parameters, streamCount.Value, true);
                else
                    return client.SerialStreamObjectsInternal(parameters, streamCount.Value, true); //There are other cmdlets after us; do one request at a time
            }
            else
            {
                var objs = GetObjectsInternal(parameters);

                GetAdditionalRecords(objs, parameters);

                return objs;
            }
        }

        internal virtual List<TObject> GetObjectsInternal(TParam parameters)
        {
            return client.GetObjects<TObject>(parameters);
        }

        /// <summary>
        /// Retrieves additional records not included in the initial request.
        /// </summary>
        /// <param name="objs">The list of records that were returned from the initial request.</param>
        /// <param name="parameters">The parameters that were used to perform the initial request.</param>
        protected virtual void GetAdditionalRecords(List<TObject> objs, TParam parameters)
        {
        }
    }
}
