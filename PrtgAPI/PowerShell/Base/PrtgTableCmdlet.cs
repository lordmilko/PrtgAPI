using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PrtgAPI.Attributes;
using PrtgAPI.Objects.Shared;
using PrtgAPI.Parameters;

namespace PrtgAPI.PowerShell.Base
{
    /// <summary>
    /// Base class for all cmdlets that return advanced objects found in tables (sensors, devices, probes, logs, etc).
    /// </summary>
    /// <typeparam name="TObject">The type of objects that will be retrieved.</typeparam>
    /// <typeparam name="TParam">The type of parameters to use to retrieve objects</typeparam>
    public abstract class PrtgTableCmdlet<TObject, TParam> : PrtgObjectCmdlet<TObject>, IStreamableCmdlet<PrtgTableCmdlet<TObject, TParam>, TObject, TParam>
        where TParam : TableParameters<TObject>
        where TObject : ObjectTable
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
        /// Initializes a new instance of the <see cref="PrtgTableCmdlet{TObject,TParam}"/> class. 
        /// </summary>
        /// <param name="content">The type of content this cmdlet will retrieve.</param>
        /// <param name="streamThreshold">The numeric threshold at which this cmdlet should show a progress bar when retrieving results.</param>
        /// <param name="streamSerial">Indicates that if the current cmdlet streams, it should do so one at a time instead of all at once.</param>
        protected PrtgTableCmdlet(Content content, int? streamThreshold, bool streamSerial = false)
        {
            ((IStreamableCmdlet<PrtgTableCmdlet<TObject, TParam>, TObject, TParam>)this).StreamProvider = new StreamableCmdletProvider<PrtgTableCmdlet<TObject, TParam>, TObject, TParam>(this, streamThreshold, streamSerial);
            this.content = content;
        }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            ValidateParameters();

            var parameters = ProcessParameters();
            var records = GetRecordsInternal(parameters);

            WriteList(records);

            //Clear the filters for the next element on the pipeline, which will simply reuse the existing PrtgTableCmdlet object
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
                var property = typeof (TObject).GetProperties().FirstOrDefault(p => p.GetCustomAttributes<PropertyParameterAttribute>().Any(a => a.Name == filter.Property.ToString()));

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
                records = GetResultsWithVariableProgress(() => GetFilteredObjects(parameters));
            else if (ProgressManager.GetResultsWithProgress)
                records = GetResultsWithProgress(() => GetFilteredObjects(parameters));
            else
            {
                if (StreamProvider.StreamResults || StreamProvider.ForceStream)
                    records = StreamProvider.StreamResultsWithProgress(parameters, Count, () => GetFilteredObjects(parameters));
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
            ValidateFilters();
            
            if (Filter != null)
                StreamProvider.StreamResults = false;

            if (Count != null)
            {
                parameters.Count = Count.Value;
                StreamProvider.StreamResults = false;
            }

            if (StreamProvider.StreamResults && ProgressManager.PartOfChain && !ProgressManager.FirstInChain)
                StreamProvider.StreamResults = false;

            return parameters;
        }

        /// <summary>
        /// Processes additional parameters specific to the current cmdlet.
        /// </summary>
        protected virtual void ProcessAdditionalParameters()
        {
        }

        internal virtual void ValidateFilters()
        {
            if (Filter != null)
            {
                var filters = Filter.ToList();

                var safeProperties = new[]
                {
                    Property.ParentId
                };

                filters = filters.Where(f => safeProperties.All(p => p != f.Property)).ToList();

                if (filters.Count == 1 && filters.Single().Property == Property.Name && Name != null)
                {
                    if (!Name.Any(n => n.Contains("*")))
                        filters.Single().Operator = FilterOperator.Equals;
                }
            }
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

        /// <summary>
        /// Display the initial progress message for the first cmdlet in a chain.<para/>
        /// If the cmdlet is streaming results, will display "detecting total number of items" and return the total number of items that will be retrieved.
        /// </summary>
        /// <returns>If the cmdlet is streaming, the total number of objects that will be retrieved. Otherwise, -1.</returns>
        protected override int DisplayFirstInChainMessage()
        {
            if (StreamProvider.StreamResults)
            {
                ProgressManager.WriteProgress($"PRTG {GetTypeDescription(typeof(TObject))} Search", "Detecting total number of items");
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
        /// <returns></returns>
        protected override IEnumerable<TObject> GetCount(IEnumerable<TObject> records, ref int count)
        {
            if (StreamProvider.StreamResults)
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
            var parts = CleanWildcard(value);

            var trimmed = string.Join(",", parts);

            //If another filter has been specified, an equals filter will become case sensitive. To work around this, we always do "contains", and then filter for
            //what we really wanted once the response is returned
            var filter = new SearchFilter(property, FilterOperator.Contains, trimmed);

            AddToFilter(filter);
        }

        internal List<string> CleanWildcard(string str)
        {
            return str.Split('*').Where(p => p != string.Empty).ToList();
        }

        private void AddToFilter(SearchFilter filter, bool invalidatesStream = true)
        {
            if (invalidatesStream)
                StreamProvider.StreamResults = false;

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
            if (StreamProvider.StreamResults || StreamProvider.ForceStream)
            {
                return StreamProvider.StreamRecords<TObject>(parameters, Count);
            }
            else
            {
                var objs = GetObjectsInternal(parameters);

                objs.AddRange(GetAdditionalRecords(parameters));

                return objs;
            }
        }

        internal virtual List<TObject> GetObjectsInternal(TParam parameters) => client.GetObjects<TObject>(parameters);

        /// <summary>
        /// Retrieves additional records not included in the initial request.
        /// </summary>
        /// <param name="parameters">The parameters that were used to perform the initial request.</param>
        protected virtual List<TObject> GetAdditionalRecords(TParam parameters) => new List<TObject>();

        #region IStreamableCmdlet

        List<TObject> IStreamableCmdlet<PrtgTableCmdlet<TObject, TParam>, TObject, TParam>.GetStreamObjects(TParam parameters) =>
            client.GetObjects<TObject>(parameters);

        async Task<List<TObject>> IStreamableCmdlet<PrtgTableCmdlet<TObject, TParam>, TObject, TParam>.GetStreamObjectsAsync(TParam parameters) =>
            await client.GetObjectsAsync<TObject>(parameters).ConfigureAwait(false);

        int IStreamableCmdlet<PrtgTableCmdlet<TObject, TParam>, TObject, TParam>.GetStreamTotalObjects(TParam parameters) =>
            client.GetTotalObjects(content);

        StreamableCmdletProvider<PrtgTableCmdlet<TObject, TParam>, TObject, TParam> IStreamableCmdlet<PrtgTableCmdlet<TObject, TParam>, TObject, TParam>.StreamProvider { get; set; }

        internal StreamableCmdletProvider<PrtgTableCmdlet<TObject, TParam>, TObject, TParam> StreamProvider => (
            (IStreamableCmdlet<PrtgTableCmdlet<TObject, TParam>, TObject, TParam>)this).StreamProvider;

        #endregion
    }
}
