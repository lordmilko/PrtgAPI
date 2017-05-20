using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.Helpers;
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

            ProgressSettings progressSettings = null;


            //var downstreamCmdlet = GetDownstreamCmdlet();

            //if (downstreamCmdlet?.ModuleName == MyInvocation.MyCommand.ModuleName)
            {
                //we're piping to something else in prtgapi. we should show progress

                //if get-channel pipes to something, how will it show progress - cos it doesnt inherit from tablecmdlet? do we need to have progress stuff in a baser class? how do we force
                //cmdlets like get-channel to call into the method in the base class? will it work like get-sensor calls into prtgtablecmdlet?

                //what if we pipe a variable containing a whole array to get-trigger?
            }

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

            if (streamResults || pipeToPrtgCmdlet || RunningPrtgCmdlets > 1)
            {
                records = GetResultsWithProgress(out progressSettings, parameters);
            }
            else
            {
                records = GetFilteredObjects(parameters);
            }

            records = FilterResponseRecords(records, Name, r => r.Name);
            records = FilterResponseRecordsByTag(records);

            WriteList(records, progressSettings);

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

        private IEnumerable<TObject> GetResultsWithProgress(out ProgressSettings progressSettings, TParam parameters)
        {
            ProgressRecord progress = null;
            int? count = null;
            long sourceId = 0;
            if (progressThreshold != null && !(RunningPrtgCmdlets > 1))
            {
                //progress = new ProgressRecord(1, $"PRTG {content} Search", "Detecting total number of items");
                progress = new ProgressRecord(RunningPrtgCmdlets, $"PRTG {content} Search", "Detecting total number of items");

                if (RunningPrtgCmdlets > 1)
                {
                    //what if we're the middle cmdlet - how will we know we have the right ID?
                    sourceId = CommandRuntime.GetLastProgressSourceId();
                    progress.ParentActivityId = RunningPrtgCmdlets - 1;
                }

                WriteProgressEx(progress, sourceId);

                count = client.GetTotalObjects(content);
            }

            progressSettings = CreateProgressSettings();

            //when we're the first element, and we're piping to a prtg cmdlet, we need to show some initial progress

            if (pipeToPrtgCmdlet && RunningPrtgCmdlets == 1)
            {
                //sourceId = CommandRuntime.GetLastProgressSourceId();

                var rec = new ProgressRecord(RunningPrtgCmdlets, progressSettings.ActivityName, progressSettings.InitialDescription);

                if (RunningPrtgCmdlets > 1)
                    rec.ParentActivityId = RunningPrtgCmdlets - 1;

                ProgressRecords.Push(rec);

                WriteProgressEx(rec, sourceId);
            }

            if (RunningPrtgCmdlets > 1)
            {
                sourceId = CommandRuntime.GetLastProgressSourceId();

                var record = ProgressRecords.Peek();

                record.CurrentOperation = $"Retrieving all {typeof(TObject).Name.ToLower()}s";

                WriteProgressEx(record, sourceId);

                
            }

            var records = streamResults ? GetObjects(parameters) : GetFilteredObjects(parameters) ;

            if (progress != null) //Remove "Detecting total number of items"
            {
                progress.RecordType = ProgressRecordType.Completed;
                WriteProgressEx(progress, sourceId);
            }

            if (count == null) //Our IEnumerable is already a List
            {
                var list = records.ToList();

                if (pipeToPrtgCmdlet)
                    progressSettings.TotalRecords = list.Count;
                else
                    progressSettings = null;

                return list.Select(s => s);
            }
            else //Our IEnumerable contains a series of Tasks; we don't want to enumerate the collection
            {
                if (count <= progressThreshold)
                    progressSettings = null;
                else
                    progressSettings.TotalRecords = count.Value;

                return records;
            }
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
