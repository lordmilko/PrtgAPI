using System.Linq;

namespace PrtgAPI.PowerShell.Progress
{
    class AbortProgressParser
    {
        public bool AbortProgress(ProgressManager manager)
        {
            if (manager.downstreamSelectObjectManager == null)
                return false;

            if (manager.UnsupportedSelectObjectProgress)
                return true;

            var firstCmdlet = manager.downstreamSelectObjectManager.Commands.First();

            if (firstCmdlet.HasFirst)
                return AbortProgressFirst(manager, manager.downstreamSelectObjectManager);
            if (firstCmdlet.HasLast)
                return AbortProgressLast(manager, manager.downstreamSelectObjectManager);
            if (firstCmdlet.HasSkip)
                return AbortProgressSkip(manager, manager.downstreamSelectObjectManager);
            if (firstCmdlet.HasSkipLast)
                return AbortProgressSkipLast(manager, manager.downstreamSelectObjectManager);
            if (firstCmdlet.HasIndex)
                return AbortProgressIndex(manager, manager.downstreamSelectObjectManager);

            return false;
        }

        public bool NeedsCompleting(ProgressManager manager)
        {
            if (manager.downstreamSelectObjectManager == null)
                return false;

            var firstCmdlet = manager.downstreamSelectObjectManager.Commands.First();

            if (firstCmdlet.HasFirst)
                return FirstNeedsCompleting(firstCmdlet, manager);
            if (firstCmdlet.HasIndex)
                return IndexNeedsCompleting(firstCmdlet, manager);

            return false;
        }

        private bool FirstNeedsCompleting(SelectObjectDescriptor firstCmdlet, ProgressManager manager)
        {
            if (manager.LastPrtgCmdletInPipeline)
            {
                if (manager.PipeFromVariableWithProgress)
                {
                    if (manager.EntirePipeline.CurrentIndex + 1 == firstCmdlet.First)
                        return true;
                }
                else
                {
                    if (manager.RecordsProcessed == firstCmdlet.First)
                        return true;
                }

            }

            return false;
        }

        private bool IndexNeedsCompleting(SelectObjectDescriptor firstCmdlet, ProgressManager manager)
        {
            if (manager.LastPrtgCmdletInPipeline)
            {
                if (manager.PipeFromVariableWithProgress)
                {
                    if (manager.EntirePipeline.CurrentIndex + 1 == firstCmdlet.Index.Last() + 1)
                        return true;
                }
                else
                {
                    if (manager.RecordsProcessed == firstCmdlet.Index.Last() + 1)
                        return true;
                }
            }

            return false;
        }

        private bool AbortProgressFirst(ProgressManager manager, SelectObjectManager selectObject)
        {
            if (selectObject.First < manager.RecordsProcessed)
                return true;

            return false;
        }

        private bool AbortProgressLast(ProgressManager manager, SelectObjectManager selectObject)
        {
            if (manager.RecordsProcessed > manager.TotalRecords - selectObject.Last)
                return true;

            return false;
        }

        private bool AbortProgressSkip(ProgressManager manager, SelectObjectManager selectObject)
        {
            return false;
        }

        private bool AbortProgressSkipLast(ProgressManager manager, SelectObjectManager selectObject)
        {
            if (manager.RecordsProcessed == manager.TotalRecords - selectObject.TotalSkipLast)
            {
                manager.CompleteProgress();
                return true;
            }

            return false;
        }

        private bool AbortProgressIndex(ProgressManager manager, SelectObjectManager selectObject)
        {
            if (selectObject.Index.Any(i => i == manager.RecordsProcessed - 1))
                return false;

            return true;
        }
    }
}
