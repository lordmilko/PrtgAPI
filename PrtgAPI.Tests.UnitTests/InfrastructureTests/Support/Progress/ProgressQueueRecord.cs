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
            var op = ProgressRecord.CurrentOperation != null ? $"{ProgressRecord.StatusDescription}: {ProgressRecord.CurrentOperation}" : ProgressRecord.StatusDescription;

            return $"{ProgressRecord.Activity}: {op} ({ProgressRecord.RecordType})";
        }
    }
}