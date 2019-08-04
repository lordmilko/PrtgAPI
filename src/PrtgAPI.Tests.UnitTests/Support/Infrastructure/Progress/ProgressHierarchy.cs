using System.Collections.Generic;
using System.Management.Automation;

namespace PrtgAPI.Tests.UnitTests.Support.Progress
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
