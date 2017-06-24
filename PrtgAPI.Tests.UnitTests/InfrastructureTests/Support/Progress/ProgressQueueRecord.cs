using System.Management.Automation;
using PrtgAPI.PowerShell;

namespace PrtgAPI.Tests.UnitTests.InfrastructureTests.Support.Progress
{
    public class ProgressQueueRecord
    {
        public ProgressRecord ProgressRecord { get; }

        public bool ContainsProgress => ProgressRecord.Activity != ProgressManager.DefaultActivity && (ProgressRecord.StatusDescription != ProgressManager.DefaultDescription);

        //public bool ChildRecord { get; }

        public ProgressQueueRecord(ProgressRecord progressRecord)
        {
            ProgressRecord = ProgressManager.CloneRecord(progressRecord);

            //ChildRecord = childRecord;
        }

        public override string ToString()
        {
            return $"{ProgressRecord.Activity}: {ProgressRecord.CurrentOperation ?? ProgressRecord.StatusDescription} ({ProgressRecord.RecordType})";
        }
    }
}