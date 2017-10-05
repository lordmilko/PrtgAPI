using System.Management.Automation;
using PrtgAPI.PowerShell.Progress;

namespace PrtgAPI.Tests.UnitTests.InfrastructureTests.Support.Progress
{
    public class ProgressQueueRecord
    {
        public ProgressRecord ProgressRecord { get; }

        public bool ContainsProgress => ProgressRecord.Activity != ProgressManager.DefaultActivity && (ProgressRecord.StatusDescription != ProgressManager.DefaultDescription);

        public ProgressQueueRecord(ProgressRecordEx progressRecord)
        {
            ProgressRecord = ProgressManager.CloneRecord(progressRecord);
        }

        public override string ToString()
        {
            return $"{ProgressRecord.Activity}: {ProgressRecord.CurrentOperation ?? ProgressRecord.StatusDescription} ({ProgressRecord.RecordType})";
        }
    }
}