using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;
using PrtgAPI.Objects.Shared;

namespace PrtgAPI.PowerShell.Base
{
    /// <summary>
    /// Base class for all cmdlets that return advanced objects found in tables (sensors, devices, probes, etc)
    /// </summary>
    /// <typeparam name="T">The type of objects that will be retrieved.</typeparam>
    public abstract class PrtgTableCmdlet<T> : PrtgObjectCmdlet<T> where T : ObjectTable
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
        /// The type of content this cmdlet will retrieve.
        /// </summary>
        protected Content content;

        private int? progressThreshold;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrtgTableCmdlet{T}"/> class. 
        /// </summary>
        /// <param name="content">The type of content this cmdlet will retrieve.</param>
        /// <param name="progressThreshold">The numeric threshold at which this cmdlet should show a progress bar when retrieving results.</param>
        protected PrtgTableCmdlet(Content content, int? progressThreshold)
        {
            this.content = content;
            this.progressThreshold = progressThreshold;
        }

        /// <summary>
        /// Provides a record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            string str = "";

            if (Filter == null)
                str = "Empty!";
            else
                str = Filter.Count().ToString();

            Debug.WriteLine($"############################ ProcessRecord for {GetType()} Filter is {str} ############################ ");

            IEnumerable<T> records = null;

            ProgressSettings progressSettings = null;

            ProcessIdFilter();
            ProcessNameFilter();
            ProcessTagsFilter();

            if (Filter == null)
            {
                ProgressRecord progress = null;
                int? count = null;

                if (progressThreshold != null)
                {
                    progress = new ProgressRecord(1, $"PRTG {content} Search", "Detecting total number of items");
                    WriteProgress(progress);
                    count = client.GetTotalObjects(content);
                }

                records = GetRecords();

                progressSettings = CompleteTotalsProgressAndCreateProgressSettings(progress, count);
            }
            else
            {
                records = GetRecords(Filter);
            }

            /*if (Name != null)
            {
                var filter = new WildcardPattern(Name.ToLower());
                records = records.Where(r => filter.IsMatch(r.Name.ToLower()));
            }*/

            records = FilterResponseRecords(records, Name, r => r.Name);

            if (Tags != null)
            {
                foreach (var tag in Tags)
                {
                    //if any of our tags are ismatch, include that record
                    var filter = new WildcardPattern(tag.ToLower());
                    records = records.Where(r => r.Tags.Any(r1 => filter.IsMatch(r1.ToLower())));
                }
            }

            try
            {
                WriteList(records, progressSettings);
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException != null)
                    throw ex.InnerException;
            }
            
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

        private IEnumerable<T> FilterResponseRecords(IEnumerable<T> records, string pattern, Func<T, string> getProperty)
        {
            if (pattern != null)
            {
                var filter = new WildcardPattern(pattern.ToLower());
                records = records.Where(r => filter.IsMatch(getProperty(r).ToLower()));
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

        

        /*private void old()
        {
            if (Filter != null)
            {
                records = GetRecords(Filter);
            }
            if (Id != null)
            {
                SetPipelineFilter(Property.ObjId, Id);
                records = GetRecords(Filter);
            }
            if (!string.IsNullOrEmpty(Name))
            {
                records = GetRecordsFilteredOnName();//instead of doing the * filtering here we should set a flag or maybe check if name was null and do it AFTER all the if statements
            }
            if (Filter == null && Id == null && string.IsNullOrEmpty(Name))
            {
                if (progressThreshold != null)
                    recordsAsync = GetRecordsAsync();
                else
                    records = GetRecords();

                showProgress = true;
            }

            ProgressSettings settings = null;

            //todo - allow doing something like get-device|get-sensor ping

            if (progressThreshold < maxRecords && showProgress)
            {
                settings = new ProgressSettings
                {
                    ActivityName = "PRTG Sensor Search",
                    InitialDescription = "Retrieving all sensors",
                    TotalRecords = maxRecords
                };
            }

            if (recordsAsync == null)
                WriteList(records, settings);
            else
                WriteList(recordsAsync.Result, settings);
        }*/

        /*private IEnumerable<T> GetRecordsFilteredOnName()
        {
            var trimmed = Name.Trim('*');

            bool ignoreFront = false;
            bool ignoreEnd = false;

            if (Name.StartsWith("*"))
                ignoreFront = true;

            if (Name.EndsWith("*"))
                ignoreEnd = true;

            
            //find out why my test wasnt working i talked about in readme.md

            


            var records = GetRecords(new[] { filter });

            if (!ignoreFront)
                records = records.Where(record => record.Name.ToLower().StartsWith(trimmed.ToLower())).ToList();

            if (!ignoreEnd)
                records = records.Where(record => record.Name.ToLower().EndsWith(trimmed.ToLower())).ToList();

            return records;
        }*/

        private void AddToFilter(params SearchFilter[] filters)
        {
            Filter = Filter?.Concat(filters).ToArray() ?? filters;
        }
        
        /// <summary>
        /// Retrieves a list of objects from a PRTG Server based on a specified filter.
        /// </summary>
        /// <param name="filter">A list of filter to use to limit search results.</param>
        /// <returns>A list of objects that match the specified search criteria.</returns>
        protected abstract IEnumerable<T> GetRecords(SearchFilter[] filter);
    }
}
