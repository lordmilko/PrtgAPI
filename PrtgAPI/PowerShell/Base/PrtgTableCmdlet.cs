using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.Objects.Shared;
using PrtgAPI.Parameters;

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
        public string Name { get; set; }

        /// <summary>
        /// <para type="description">Retrieve an object with a specified ID.</para>
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Retrieve an obejct with a specified ID.")]
        public int? Id { get; set; }

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
        public SensorStatus[] Status { get; set; }

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
            var parameters = CreateParameters();

            IEnumerable<TObject> records;

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

            if (streamResults && !ProgressManager.FirstInChain)
                streamResults = false;

            if (ProgressManager.PartOfChain && PrtgSessionState.EnableProgress)
                records = GetResultsWithProgress(() => GetFilteredObjects(parameters));
            else
            {
                if (streamResults)
                    records = StreamResultsWithProgress();
                else
                    records = GetFilteredObjects(parameters);
            }

            records = FilterResponseRecords(records, Name, r => r.Name);
            records = FilterResponseRecordsByTag(records);

            WriteList(records);

            //Clear the filters for the next element on the pipeline, which will simply reuse the existing PrtgTableCmdlet object

            Filter = null;
        }

        private IEnumerable<TObject> GetFilteredObjects(TParam parameters)
        {
            if (Filter != null)
                parameters.SearchFilter = Filter;
            return GetObjects(parameters);
        }

        private void ProcessIdFilter()
        {
            if (Id != null)
            {
                AddPipelineFilter(Property.ObjId, Id);
            }
        }

        private void ProcessNameFilter()
        {
            if (!string.IsNullOrEmpty(Name))
            {
                AddWildcardFilter(Property.Name, Name);
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

        //todo: we need to write some unit tests for this progress business. we will need to define 

        private IEnumerable<TObject> StreamResultsWithProgress()
        {
            ProgressManager.WriteProgress($"PRTG {content} Search", "Detecting total number of items");

            var count = client.GetTotalObjects(content);
            var records = GetRecords();

            ProgressManager.CompleteProgress();

            if (count > progressThreshold)
            {
                SetObjectSearchProgress(ProcessingOperation.Retrieving, count);
            }

            return records;
        }

        protected override int DisplayFirstInChainMessage()
        {
            int count = -1;

            if (streamResults)
            {
                ProgressManager.WriteProgress($"PRTG {content} Search", "Detecting total number of items");
                count = client.GetTotalObjects(content);
            }
            else
                base.DisplayFirstInChainMessage();

            return count;
        }

        protected override IEnumerable<TObject> GetCount(IEnumerable<TObject> records, ref int count)
        {
            if (streamResults)
                return records;
            else
                return base.GetCount(records, ref count);
        }

        private IEnumerable<TObject> FilterResponseRecordsByTag(IEnumerable<TObject> records)
        {
            if (Tags != null)
            {
                foreach (var tag in Tags)
                {
                    //if any of our tags are ismatch, include that record
                    var filter = new WildcardPattern(tag.ToLower());
                    records = records.Where(r => r.Tags.Any(r1 => filter.IsMatch(r1.ToLower())));
                }
            }

            return records;
        }

        private ProgressSettings CreateProgressSettings() //TODO - RENAME
        {
            ProgressSettings progressSettings = null;

            progressSettings = new ProgressSettings()
            {
                ActivityName = $"PRTG {typeof(TObject).Name} Search",
                InitialDescription = $"Retrieving all {typeof(TObject).Name.ToLower()}s",
            };

            return progressSettings;
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
                return client.StreamObjects(parameters);
            else
                return client.GetObjects<TObject>(parameters);
        }

        //protected abstract IEnumerable<TObject> GetRecords(TParam parameters);
    }
}
