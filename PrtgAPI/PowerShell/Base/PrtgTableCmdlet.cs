using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;
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
        /// Filter the response to objects with a certain name. Can include wildcards.
        /// </summary>
        [Parameter(Mandatory = false, Position = 0, HelpMessage = "Filter the response to objects with a certain name. Can include wildcards.")]
        public string Name { get; set; }

        /// <summary>
        /// Retrieve an object with a specified ID.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Retrieve an obejct with a specified ID.")]
        public int? Id { get; set; }

        /// <summary>
        /// Filter the response to objects that match one or more criteria.
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true, Position = 1, HelpMessage = "Filter the response to objects that match one or more criteria.")]
        public SearchFilter[] Filter { get; set; }

        /// <summary>
        /// Filter the response to objects with certain tags. Can include wildcards.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Filter the response to objects with certain tags. Can include wildcards.")]
        public string[] Tags { get; set; }

        /// <summary>
        /// Maximum number of results to return.
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
        /// Provides a record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            var parameters = CreateParameters();

            IEnumerable<TObject> records;

            ProgressSettings progressSettings = null;

            ProcessIdFilter();
            ProcessNameFilter();
            ProcessTagsFilter();

            if (Count != null)
            {
                parameters.Count = Count.Value;
                streamResults = false;
            }

            if (streamResults)
            {
                records = GetResultsWithProgress(out progressSettings);
            }
            else
            {
                if(Filter != null)
                    parameters.SearchFilter = Filter;
                records = GetObjects(parameters);
            }

            records = FilterResponseRecords(records, Name, r => r.Name);
            records = FilterResponseRecordsByTag(records);

            WriteList(records, progressSettings);

            //Clear the filters for the next element on the pipeline, which will simply reuse the existing PrtgTableCmdlet object

            Filter = null;
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

        private IEnumerable<TObject> GetResultsWithProgress(out ProgressSettings progressSettings)
        {
            ProgressRecord progress = null;
            int? count = null;

            if (progressThreshold != null)
            {
                progress = new ProgressRecord(1, $"PRTG {content} Search", "Detecting total number of items");
                WriteProgress(progress);
                count = client.GetTotalObjects(content);
            }

            var records = GetRecords();

            progressSettings = CompleteTotalsProgressAndCreateProgressSettings(progress, count);

            return records;
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

        private ProgressSettings CompleteTotalsProgressAndCreateProgressSettings(ProgressRecord progress, int? count) //TODO - RENAME
        {
            ProgressSettings progressSettings = null;

            if (progress != null)
            {
                progress.RecordType = ProgressRecordType.Completed;
                WriteProgress(progress);

                if (count > progressThreshold)
                {
                    progressSettings = new ProgressSettings()
                    {
                        ActivityName = $"PRTG {content.ToString().Substring(0, content.ToString().Length - 1)} Search",
                        InitialDescription = $"Retrieving all {content.ToString().ToLower()}",
                        TotalRecords = count.Value
                    };
                }
            }

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
