using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace PrtgAPI.PowerShell.Progress
{
    class ProgressPipeline
    {
        private Stack<ProgressRecordEx> progressRecords = new Stack<ProgressRecordEx>();

        internal object FirstCmdletInPipeline { get; private set; }

        internal int Count => progressRecords.Count;

        internal ProgressRecordEx Peek()
        {
            return progressRecords.Peek();
        }

        internal ProgressRecordEx CurrentRecord => progressRecords.Peek();

        internal ProgressRecordEx PreviousRecord => progressRecords.Skip(1).FirstOrDefault();

        internal ProgressPipeline(string defaultActivity, string defaultDescription, object firstCmdletInPipeline, int offset, long sourceId)
        {
            Push(defaultActivity, defaultDescription, offset, sourceId);
            FirstCmdletInPipeline = firstCmdletInPipeline;
        }

        internal void Push(string defaultActivity, string defaultDescription, int offset, long sourceId)
        {
            progressRecords.Push(new ProgressRecordEx(offset + 1, defaultActivity, defaultDescription, sourceId));
        }

        internal void Pop()
        {
            progressRecords.Pop();
        }
    }
}