using PrtgAPI.PowerShell.Progress;

namespace PrtgAPI.Tests.UnitTests.Support.Progress
{
    public class ProgressQueueRecord
    {
        public ProgressRecordEx ProgressRecord { get; }

        public bool ContainsProgress => ProgressRecord?.ContainsProgress == true;

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