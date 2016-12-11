using System.Management.Automation;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// Filters results on the PRTG Server to improve performance.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "SearchFilter")]
    public class NewSearchFilter : PrtgCmdlet
    {
        /// <summary>
        /// Object property to filter on.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0)]
        public Property Property { get; set; }

        /// <summary>
        /// Operator to filter with.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 1)]
        public FilterOperator Operator { get; set; }

        /// <summary>
        /// Value to filter for.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 2)]
        public string Value { get; set; }

        /// <summary>
        /// Provides a record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            WriteObject(new SearchFilter(Property, Operator, Value));
        }
    }
}
