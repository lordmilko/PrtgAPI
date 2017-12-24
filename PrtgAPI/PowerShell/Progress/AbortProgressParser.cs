using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.PowerShell.Progress
{
    class AbortProgressParser
    {
        public bool AbortProgress(ProgressManager manager)
        {
            if (manager.downstreamSelectObjectManager == null)
                return false;

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

        private bool AbortProgressFirst(ProgressManager manager, SelectObjectManager selectObject)
        {
            if (selectObject.HasLast)
            {
                if (manager.recordsProcessed > manager.TotalRecords - selectObject.Last)
                    return true;
            }

            if (selectObject.HasSkip)
            {
                return false;
            }

            if (selectObject.HasSkipLast)
            {
                if (manager.PipeFromVariableWithProgress)
                {
                }
                else
                {
                    //12.3c: Table -> Select -First -> Select -SkipLast -> Table
                    if (manager.recordsProcessed > selectObject.First - selectObject.SkipLast && selectObject.SplitLastOverTwo)
                    {
                        manager.CompleteProgress();
                        return true;
                    }

                    return false;
                }
            }

            if (selectObject.HasIndex)
            {
                if (selectObject.Index.Any(i => i == manager.recordsProcessed - 1))
                    return false;

                return true;
            }

            if (selectObject.First < manager.recordsProcessed)
                return true;

            return false;
        }

        private bool AbortProgressLast(ProgressManager manager, SelectObjectManager selectObject)
        {
            if (selectObject.HasSkip)
            {

            }

            if (selectObject.HasSkipLast)
            {

            }

            if (selectObject.HasIndex)
            {

            }

            if (manager.recordsProcessed > manager.TotalRecords - selectObject.Last)
                return true;

            return false;
        }

        private bool AbortProgressSkip(ProgressManager manager, SelectObjectManager selectObject)
        {
            if (selectObject.HasFirst)
            {
            }

            if (selectObject.HasLast)
            {
                if (manager.recordsProcessed > manager.TotalRecords - selectObject.TotalSkip)
                    return true;
            }

            if (selectObject.HasSkipLast)
            {
                if (selectObject.HasSkip)
                    manager.CompleteProgress();

                if (manager.recordsProcessed == manager.TotalRecords - selectObject.TotalSkipLast)
                    manager.CompleteProgress();

                return true;
            }

            if (selectObject.HasIndex)
            {
                if (selectObject.Index.Any(i => i + selectObject.TotalSkip == manager.recordsProcessed - 1))
                    return false;

                return true;
            }

            return false;
        }

        private bool AbortProgressSkipLast(ProgressManager manager, SelectObjectManager selectObject)
        {
            if (selectObject.HasFirst)
            {
            }

            if (selectObject.HasLast)
            {
            }

            if (selectObject.HasSkip)
            {
                manager.CompleteProgress();

                return true;
            }

            if (selectObject.HasSkipLast)
            {
            }

            if (selectObject.HasIndex)
            {

            }

            if (manager.recordsProcessed == manager.TotalRecords - selectObject.TotalSkipLast)
                manager.CompleteProgress();

            return true;
        }

        private bool AbortProgressIndex(ProgressManager manager, SelectObjectManager selectObject)
        {
            if (selectObject.HasFirst)
            {
            }

            if (selectObject.HasLast)
            {
                var last = selectObject.Index.OrderBy(i => i).Skip(selectObject.Index.Length - selectObject.Last).ToList();

                if (last.Any(i => i == manager.recordsProcessed - 1))
                    return false;

                return true;
            }

            if (selectObject.HasSkip)
            {
            }

            if (selectObject.HasSkipLast)
            {
            }

            if (selectObject.HasIndex)
            {
            }

            if (selectObject.Index.Any(i => i == manager.recordsProcessed - 1))
                return false;

            return true;
        }
    }
}
