using System.Collections.Generic;

namespace PrtgAPI.PowerShell.Progress
{
    class ProgressState
    {
        public ProgressRecordEx ProgressRecord { get; }

        public void Add(object obj)
        {
            Records.Add(obj);
            Current = obj;
        }

        public List<object> Records { get; set; } = new List<object>();

        public object Current { get; set; }

        public int? TotalRecords { get; set; }

        public ProgressState(int offset, long sourceId)
        {
            ProgressRecord = new ProgressRecordEx(offset + 1, sourceId);
        }
    }
}
