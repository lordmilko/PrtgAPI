using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.Helpers;

namespace PrtgAPI.PowerShell.Progress
{
    class ProgressPipelineStack
    {
        private Stack<ProgressPipeline> progressPipelines = new Stack<ProgressPipeline>();

        private ProgressPipeline currentPipeline => progressPipelines.Peek();

        internal int Count => progressPipelines.Count;

        internal int RecordsInCurrentPipeline => currentPipeline.Count;

        internal ProgressRecordEx CurrentRecordInPipeline => currentPipeline.CurrentRecord;

        internal ProgressRecordEx PreviousRecordInPipeline => currentPipeline.PreviousRecord;

        internal void Push(string defaultActivity, string defaultDescription, PSCmdlet cmdlet, long sourceId)
        {
            var firstCmdletInPipeline = cmdlet.GetFirstObjectInPipeline();

            var offset = progressPipelines.Sum(f => f.Count);

            if (progressPipelines.Count == 0)
            {
                progressPipelines.Push(new ProgressPipeline(defaultActivity, defaultDescription, firstCmdletInPipeline, offset, sourceId));
            }
            else
            {
                if (currentPipeline.FirstCmdletInPipeline.Equals(firstCmdletInPipeline))
                {
                    currentPipeline.Push(defaultActivity, defaultDescription, offset, sourceId);
                }
                else
                {
                    progressPipelines.Push(new ProgressPipeline(defaultActivity, defaultDescription, firstCmdletInPipeline, offset, sourceId));
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