using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.Helpers;
using PrtgAPI.Objects.Shared;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Progress;

namespace PrtgAPI.PowerShell.Base
{
    /// <summary>
    /// Base class for all cmdlets that return advanced objects found in tables (sensors, devices, probes, etc)
    /// </summary>
    /// <typeparam name="TObject">The type of objects that will be retrieved.</typeparam>
    /// <typeparam name="TParam">The type of parameters to use to retrieve objects</typeparam>
    public abstract class PrtgTableCmdlet<TObject, TParam> : PrtgObjectCmdlet<TObject> where TParam : TableParameters<TObject> where TObject : ObjectTable
    {
        /// <summary>
        /// <para type="description">Filter the response to objects with a certain name. Can include wildcards.</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 0, HelpMessage = "Filter the response to objects with a certain name. Can include wildcards.")]
        public string[] Name { get; set; }

        /// <summary>
        /// <para type="description">Retrieve an object with a specified ID.</para>
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Retrieve an obejct with a specified ID.")]
        public int?[] Id { get; set; }

        /// <summary>
        /// <para type="description">Filter the response to objects that match one or more criteria.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true, Position = 1, HelpMessage = "Filter the response to objects that match one or more criteria.")]
        public SearchFilter[] Filter { get; set; }

        /// <summary>
        /// <para type="description">Filter the response to objects with certain tags. Can include wildcards.</para>
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Filter the response to objects with certain tags. Can include wildcards.")]
        public string[] Tags { get; set; }

        /// <summary>
        /// <para type="description">Only retrieve objects that match a specific status.</para>
        /// </summary>
        [Parameter(ValueFromPipeline = true)]
        public Status[] Status { get; set; }

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

        /// <summary>
        /// Initializes a new instance of the <see cref="PrtgTableCmdlet{TObject, TParam}"/> class. 
        /// </summary>
        /// <param name="content">The type of content this cmdlet will retrieve.</param>
        /// <param name="progressThreshold">The numeric threshold at which this cmdlet should show a progress bar when retrieving results.</param>
        protected PrtgTableCmdlet(Content content, int? progressThreshold)
        {
            this.content = content;
            this.progressThreshold = progressThreshold;

            if(progressThreshold != null)
                streamResults = true;
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
                parameters.SearchFilter = Filter;
            return GetObjects(parameters);
        }

        private IEnumerable<TObject> GetRecordsInternal(TParam parameters)
        {
            IEnumerable<TObject> records;

            if (ProgressManager.PipeFromVariableWithProgress && PrtgSessionState.EnableProgress)
                records = GetResultsWithVariableProgress(() => GetFilteredObjects(parameters)); //todo: need to test this works properly
            else if (ProgressManager.PartOfChain && PrtgSessionState.EnableProgress)
                records = GetResultsWithProgress(() => GetFilteredObjects(parameters));
            else
            {
                if (streamResults)
                    records = StreamResultsWithProgress();
                else
                    records = GetFilteredObjects(parameters);
            }

            records = PostProcessRecords(records);

            return records;
        }

        private IEnumerable<TObject> PostProcessRecords(IEnumerable<TObject> records)
        {
            records = FilterResponseRecordsByName(records);
            records = FilterResponseRecordsByTag(records);

            return records;
        }

        private TParam ProcessParameters()
        {
            var parameters = CreateParameters();

            ProcessIdFilter();
            ProcessNameFilter();
            ProcessTagsFilter();

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

        private void ProcessIdFilter()
        {
            if (Id != null)
            {
                foreach (var id in Id)
                {
                    AddPipelineFilter(Property.Id, id);
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

        private void ProcessTagsFilter()
        {
            if (Tags != null)
            {
                foreach (var value in Tags)
                {
                    AddWildcardFilter(Property.Tags, value);
                }
            }
        }

        private IEnumerable<TObject> StreamResultsWithProgress()
        {
            ProgressManager.Scenario = ProgressScenario.StreamProgress;

            ProgressManager.WriteProgress($"PRTG {GetTypeDescription(typeof(TObject))} Search", "Detecting total number of items");

            var count = client.GetTotalObjects(content);
            var records = GetRecords();

            ProgressManager.CompleteProgress();

            if (count > progressThreshold)
            {
                SetObjectSearchProgress(ProcessingOperation.Retrieving, count);
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
                return client.GetTotalObjects(content);
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

        private IEnumerable<TObject> FilterResponseRecordsByTag(IEnumerable<TObject> records)
        {
            if (Tags != null)
            {
                //Select all records where at least one of the filter tags is present
                records = from record in records
                          from tag in Tags
                          let filter = new WildcardPattern(tag, WildcardOptions.IgnoreCase)
                          from t in record.Tags
                          where filter.IsMatch(t)
                          select record;
            }

            return records;
        }

        /// <summary>
        /// Adds a filter for a concrete value that came in from the pipeline (such as an Id)
        /// </summary>
        /// <param name="property">The property to filter on.</param>
        /// <param name="value">The value to filter for.</param>
        protected void AddPipelineFilter(Property property, object value)
        {
            var filter = new SearchFilter(property, FilterOperator.Equals, value);

            AddToFilter(filter);
        }

        /// <summary>
        /// Add a filter for a value that may contain wildcard characters.
        /// </summary>
        /// <param name="property">The property to filter on.</param>
        /// <param name="value">The value to filter for.</param>
        protected void AddWildcardFilter(Property property, string value)
        {
            var trimmed = value.Trim('*');

            //If another filter has been specified, an equals filter will become case sensitive. To work around this, we always do "contains", and then filter for
            //what we really wanted once the response is returned
            var filter = new SearchFilter(property, FilterOperator.Contains, trimmed);

            AddToFilter(filter);
        }

        private void AddToFilter(params SearchFilter[] filters)
        {
            streamResults = false;

            Filter = Filter?.Concat(filters).ToArray() ?? filters;
        }

        /// <summary>
        /// Creates a new parameter object capable of being passed to <see cref="GetObjects(TParam)"/> 
        /// </summary>
        /// <returns></returns>
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
            if (streamResults)
            {
                //Depending on the number of items we're streaming, we may have made so many requests to PRTG that it can't possibly
                //respond to any cmdlets further downstream until all of the streaming requests have been completed

                //As such, if there are no other PRTG cmdlets after us, stream as normal. Otherwise, only request a couple at a time
                //so the PRTG will be able to handle the next cmdlet's request
                if (ProgressManager.Scenario == ProgressScenario.StreamProgress && !this.PipelineRemainingHasCmdlet<PrtgCmdlet>()) //There are no other cmdlets after us
                    return client.StreamObjects(parameters);
                else
                    return client.SerialStreamObjects(parameters); //There are other cmdlets after us; do one request at a time
            }
            else
                return client.GetObjects<TObject>(parameters);
        }
    }
}
