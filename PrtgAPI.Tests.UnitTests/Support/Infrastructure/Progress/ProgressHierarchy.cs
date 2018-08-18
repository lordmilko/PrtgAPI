using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Tests.UnitTests.InfrastructureTests.Support.Progress
{
    public class ProgressHierarchy
    {
        public ProgressRecord Record { get; set; }

        public List<ProgressHierarchy> Children { get; set; } = new List<ProgressHierarchy>();

        public ProgressHierarchy(ProgressRecord record)
        {
            Record = record;
        }

        public override string ToString()
        {
            return $"{Record.ParentActivityId} {Record.ActivityId}";
        }
    }
}
