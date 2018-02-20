using System.Collections.Generic;
using System.Linq;

namespace PrtgAPI.PowerShell.Progress
{
    class ProgressState
    {
        public ProgressRecordEx ProgressRecord { get; }

        private IEnumerable<object> rawRecords;

        public IEnumerable<object> Records
        {
            get { return rawRecords; }
            private set
            {
                rawRecords = value;

                if (!IsLazy)
                    recordsList = rawRecords.ToList();
            }
        }

        private List<object> recordsList;

        private object obj;

        public object Current
        {
            get { return obj; }
            set
            {
                if (IsLazy)
                {
                    if (lazyRecordsProcessed < 1)
                        lazyRecordsProcessed = 1;
                    else
                        lazyRecordsProcessed++;
                }

                obj = value;
            }
        }

        public int CurrentItem
        {
            get
            {
                if (IsLazy)
                    return lazyRecordsProcessed;

                var index = recordsList.ToList().IndexOf(obj);

                if (index == -1)
                    return index;

                return index + 1;
            }
        }

        public int? TotalRecords { get; set; }

        public bool IsLazy { get; set; }

        private int lazyRecordsProcessed = -1;

        public ProgressState(int offset, long sourceId)
        {
            ProgressRecord = new ProgressRecordEx(offset + 1, sourceId);
        }

        public void RegisterRecords<T>(IEnumerable<T> records, bool isLazy)
        {
            IsLazy = isLazy;
            Records = records.Cast<object>();
        }
    }
}
