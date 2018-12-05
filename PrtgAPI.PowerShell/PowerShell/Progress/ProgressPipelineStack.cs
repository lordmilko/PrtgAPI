using System.Collections.Generic;
using System.Linq;

namespace PrtgAPI.PowerShell.Progress
{
    class ProgressPipelineStack
    {
        private Stack<ProgressPipeline> progressPipelines = new Stack<ProgressPipeline>();

        internal ProgressPipeline currentPipeline => progressPipelines.Peek();

        internal int Count => progressPipelines.Count;

        internal int RecordsInCurrentPipeline => currentPipeline.Count;

        internal ProgressState CurrentRecordInPipeline => currentPipeline.CurrentState;

        internal ProgressState PreviousRecordInPipeline => currentPipeline.PreviousState;

        internal void Push(ProgressManager manager, long sourceId)
        {
            var firstCmdletInPipeline = manager.CacheManager.GetFirstCmdletInPipeline();

            var offset = progressPipelines.Sum(f => f.Count);

            if (progressPipelines.Count == 0)
            {
                progressPipelines.Push(new ProgressPipeline(firstCmdletInPipeline, offset, sourceId));
            }
            else
            {
                if (currentPipeline.FirstCmdletInPipeline.Equals(firstCmdletInPipeline))
                {
                    currentPipeline.Push(offset, sourceId);
                }
                else
                {
                    progressPipelines.Push(new ProgressPipeline(firstCmdletInPipeline, offset, sourceId));
                }
            }
        }

        internal void Pop()
        {
            currentPipeline.Pop();

            if (RecordsInCurrentPipeline == 0)
                progressPipelines.Pop();
        }
    }
}