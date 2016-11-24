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
        [Parameter(Mandatory = false, Position = 0, HelpMessage = "Limit results to sensors those with a certain name.")]
        public string Name { get; set; }

        [Parameter(Mandatory = false)]
        public int? Id { get; set; }

        [Parameter(Mandatory = false, ValueFromPipeline = true, Position = 1)]
        public SearchFilter[] Filter { get; set; }

        protected Content content;
        private int? progressThreshold;

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
            Task<IEnumerable<T>> recordsAsync = null;

            ProgressSettings progressSettings = null;

            if (Id != null)
            {
                AddPipelineFilter(Property.ObjId, Id);
            }
            if (!string.IsNullOrEmpty(Name))
            {
                AddWildcardFilter(Name);
            }

            if (Filter == null)
            {
                //progress

                ProgressRecord progress = null;
                int? count = null;

                if (progressThreshold != null)
                {
                    progress = new ProgressRecord(1, $"PRTG {content} Search", "Detecting total number of items");
                    WriteProgress(progress);
                    count = client.GetTotalObjects(content);
                }

                records = GetRecords();

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
            }
            else
            {
                records = GetRecords(Filter);
            }

            if (Name != null)
            {
                var filter = new WildcardPattern(Name.ToLower());
                records = records.Where(r => filter.IsMatch(r.Name.ToLower()));
            }

            WriteList(records, progressSettings);

            //Clear the filters for the next element on the pipeline, which will simply reuse the existing PrtgTableCmdlet object

            Filter = null;
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

        protected void AddWildcardFilter(string value)
        {
            var trimmed = Name.Trim('*');

            bool ignoreFront = false;
            bool ignoreEnd = false;

            if (Name.StartsWith("*"))
                ignoreFront = true;

            if (Name.EndsWith("*"))
                ignoreEnd = true;

            var op = FilterOperator.Equals; //todo - my documentation in my readme.md file says equals is case sensitive, but it actually doesnt appear to be. whats up with that?

            if (ignoreFront || ignoreEnd)
                op = FilterOperator.Contains;

            var filter = new SearchFilter(Property.Name, op, trimmed);

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

        

        protected abstract IEnumerable<T> GetRecords(SearchFilter[] filter);
    }
}
