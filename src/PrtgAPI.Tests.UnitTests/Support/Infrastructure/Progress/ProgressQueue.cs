using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using PrtgAPI.PowerShell.Progress;

namespace PrtgAPI.Tests.UnitTests.Support.Progress
{
    public class ProgressQueue
    {
        public static Queue<ProgressQueueRecord> RecordQueue = new Queue<ProgressQueueRecord>();
        public static Queue<string> ProgressSnapshots { get; set; } = new Queue<string>();

        private static object queueLock = new object();

        public static void Enqueue(ProgressRecordEx progressRecord)
        {
            lock (queueLock)
            {
                RecordQueue.Enqueue(new ProgressQueueRecord(progressRecord));
            }
        }

        private static bool AnyRecords
        {
            get
            {
                lock (queueLock)
                    return RecordQueue.Any();
            }
        }

        public static string Dequeue()
        {
            if (!ProgressSnapshots.Any() && AnyRecords)
            {
                List<ProgressRecordEx> list;

                lock (queueLock)
                {
                    list = RecordQueue.Select(q => q.ProgressRecord).ToList();
                    RecordQueue.Clear();
                }

                var hierarchy = ProgressGrouper.GetHierarchy(list);
                var progressGroups = ProgressGrouper.GetProgressSnapshots(hierarchy, new List<ProgressRecord>()).Select(s => s.ToList()).ToList();
                ProgressSnapshots = GetSnapshotQueue(progressGroups);
            }

            return ProgressSnapshots.Dequeue();
        }

        private static Queue<string> GetSnapshotQueue(List<List<ProgressRecord>> progressGroups)
        {
            Queue<string> snapshotQueue = new Queue<string>();

            foreach (var snapshot in progressGroups)
            {
                var builder = new StringBuilder();

                for (int i = 0; i < snapshot.Count; i++)
                {
                    var headerIndent = new string(' ', i * 4);
                    var recordIndent = new string(' ', (i + 1) * 4);

                    builder.Append($"{headerIndent}{snapshot[i].Activity}");

                    if (snapshot[i].RecordType == ProgressRecordType.Completed)
                        builder.Append($" ({snapshot[i].RecordType})\n");
                    else
                        builder.Append("\n");

                    builder.Append($"{recordIndent}{snapshot[i].StatusDescription}\n");

                    if (snapshot[i].PercentComplete >= 0)
                    {
                        var maxChars = 40;

                        var percentChars = (int)((double)snapshot[i].PercentComplete/100*maxChars);

                        var spaceChars = maxChars - percentChars;

                        var percentBuilder = new StringBuilder();

                        percentBuilder.Append("[");

                        for (int j = 0; j < percentChars; j++)
                            percentBuilder.Append("o");

                        for (int j = 0; j < spaceChars; j++)
                            percentBuilder.Append(" ");

                        percentBuilder.Append($"] ({snapshot[i].PercentComplete}%)");

                        builder.Append($"{recordIndent}{percentBuilder}\n");

                        if (snapshot[i].SecondsRemaining != -1)
                        {
                            var timeSpan = TimeSpan.FromSeconds(snapshot[i].SecondsRemaining);

                            var str = timeSpan.ToString(@"hh\:mm\:ss");

                            builder.Append($"{recordIndent}{str} remaining\n");
                        }
                    }

                    if (snapshot[i].CurrentOperation != null)
                        builder.Append($"{recordIndent}{snapshot[i].CurrentOperation}\n");
                }

                snapshotQueue.Enqueue(builder.ToString().Trim('\n'));
            }

            return snapshotQueue;
        }
    }
}