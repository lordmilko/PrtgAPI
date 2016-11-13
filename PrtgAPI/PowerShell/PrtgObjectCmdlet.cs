using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using PrtgAPI.Objects.Shared;

namespace PrtgAPI.PowerShell
{
    public abstract class PrtgObjectCmdlet<T> : PrtgCmdlet where T : ObjectTable
    {
        [Parameter(Mandatory = false, ValueFromPipeline = true, Position = 0, HelpMessage = "Limit results to sensors those with a certain name.")]
        public string Name { get; set; }

        [Parameter(Mandatory = false, ValueFromPipeline = true, Position = 1)]
        public SearchFilter[] Filter { get; set; }

        protected override void ProcessRecord()
        {
            List<T> records = null;

            if (Filter != null)
            {
                records = GetRecords(Filter);
            }
            else if (!string.IsNullOrEmpty(Name))
            {
                records = GetRecordsFilteredOnName();
            }
            else
                records = GetRecords();

            WriteList(records);
        }

        private List<T> GetRecordsFilteredOnName()
        {
            var trimmed = Name.Trim('*');

            bool ignoreFront = false;
            bool ignoreEnd = false;

            if (Name.StartsWith("*"))
                ignoreFront = true;

            if (Name.EndsWith("*"))
                ignoreEnd = true;

            var op = FilterOperator.Equals;

            if (ignoreFront || ignoreEnd)
                op = FilterOperator.Contains;

            var filter = new SearchFilter(Property.Name, op, trimmed);

            var records = GetRecords(filter);

            if (!ignoreFront)
                records = records.Where(record => record.Name.ToLower().StartsWith(trimmed.ToLower())).ToList();

            if (!ignoreEnd)
                records = records.Where(record => record.Name.ToLower().EndsWith(trimmed.ToLower())).ToList();

            return records;
        }

        protected void SetPipelineFilter(Property property, object value)
        {
            Filter = new[] { new SearchFilter(property, FilterOperator.Equals, value) };
        }

        protected abstract List<T> GetRecords();

        protected abstract List<T> GetRecords(params SearchFilter[] filter);
    }
}
