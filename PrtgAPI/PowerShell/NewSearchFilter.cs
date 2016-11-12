using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.PowerShell
{
    [Cmdlet(VerbsCommon.New, "SearchFilter")]
    public class NewSearchFilter : PrtgCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0)]
        public Property Property { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 1)]
        public FilterOperator Operator { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 2)]
        public string Value { get; set; }

        protected override void ProcessRecord()
        {
            WriteObject(new SearchFilter(Property.Name, Operator, Value));
        }
    }
}
