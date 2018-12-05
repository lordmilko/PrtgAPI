using System.Collections.Generic;
using System.Linq;

namespace PrtgAPI.PowerShell.Progress
{
    class ProgressPipeline
    {
        private Stack<ProgressState> progressStates = new Stack<ProgressState>();

        internal List<ProgressRecordEx> Records => progressStates.Select(r => r.ProgressRecord).ToList();

        internal object FirstCmdletInPipeline { get; private set; }

        internal int Count => progressStates.Count;

        internal ProgressState Peek()
        {
            return progressStates.Peek();
        }

        internal ProgressState CurrentState => progressStates.Peek();

        internal ProgressState PreviousState => progressStates.Skip(1).FirstOrDefault();

        internal ProgressPipeline(object firstCmdletInPipeline, int offset, long sourceId)
        {
            Push(offset, sourceId);
            FirstCmdletInPipeline = firstCmdletInPipeline;
        }

        internal void Push(int offset, long sourceId)
        {
            //If two batch/passthru cmdlets are chained together via an intermediate table cmdlet, the first action cmdlet's endprocessing
            //block will end when the second's endprocessing executes, resulting in the first cmdlet to be lost from the progress pipeline

            if (progressStates.Count > 0 && CurrentState?.ProgressRecord?.ActivityId == offset + 1)
                offset++;

            progressStates.Push(new ProgressState(offset, sourceId));
        }

        internal void Pop()
        {
            progressStates.Pop();
        }
    }
}