using System;
using System.Linq;

namespace PrtgAPI.PowerShell.Progress
{
    class NotReadyParser
    {
        private SelectObjectManager selectObject;

        private ProgressManager manager;
        private ProgressManager previousManager;

        public Pipeline Pipeline => manager.Pipeline;

        public int? TotalRecords => manager.TotalRecords;
        public bool PipeFromVariableWithProgress => manager.PipeFromVariableWithProgress;
        public Pipeline EntirePipeline => manager.EntirePipeline;

        public NotReadyParser(ProgressManager manager)
        {
            this.manager = manager;

            selectObject = manager.upstreamSelectObjectManager;

            var previousCmdlet = this.manager.CacheManager.GetPreviousPrtgCmdlet();
            previousManager = previousCmdlet?.ProgressManager;
        }

        public bool NotReady()
        {
            if (manager.UnsupportedSelectObjectProgress)
                return false;

            if (manager.upstreamSelectObjectManager == null)
                return false;

            var firstCmdlet = manager.upstreamSelectObjectManager.Commands.Last();

            if (firstCmdlet.HasFirst)
                return NotReadyFirst();
            if (firstCmdlet.HasLast)
                return NotReadyLast();
            if (firstCmdlet.HasSkip)
                return NotReadySkip();
            if (firstCmdlet.HasSkipLast)
                return NotReadySkipLast();
            if (firstCmdlet.HasIndex)
                return NotReadyIndex();

            return false;
        }

        private bool NotReadyFirst()
        {
            if (PipeFromVariableWithProgress)
            {
                //12.1c: Variable -> Select -First -> Table
                if (EntirePipeline.CurrentIndex + 1 < selectObject.First)
                    return true;
            }
            else
            {
                if (previousManager.recordsProcessed == -1)
                    throw new NotImplementedException("Attempted to process a recordsProcessed that is not in use");

                //12.1a: Get-Probe -Count 3 | Select -First 2 | Get-Device
                if (previousManager.recordsProcessed < selectObject.First)
                    return true;
            }

            return false;
        }

        private bool NotReadyFirst_Skip()
        {
            if (PipeFromVariableWithProgress)
            {
                //12.6b: Variable -> Select -First -Skip -> Table
                if (Pipeline.CurrentIndex + 1 >= selectObject.First + selectObject.Skip)
                    return false;

                //12.7b: Variable -> Select -First -> Select -Skip -> Table
                if (Pipeline.CurrentIndex + 1 >= selectObject.First && selectObject.SplitOverTwo)
                    return false;
            }
            else
            {
                //12.2b: Table -> Select -First -Skip -> Table
                if (previousManager.recordsProcessed >= selectObject.First + selectObject.Skip)
                    return false;

                //12.3b: Table -> Select -First -> Select -Skip -> Table
                if (previousManager.recordsProcessed >= selectObject.First && selectObject.SplitOverTwo)
                    return false;
            }

            return true;
        }

        private bool NotReadyLast()
        {
            if (Pipeline == null)
                return false;

            //13.1a: Table -> Select -Last -> Table
            if (Pipeline.CurrentIndex + 1 < Pipeline.List.Count)
                return true;

            return false;
        }
        
        private bool NotReadySkip()
        {
            if (PipeFromVariableWithProgress) //todo: why isnt this causing a null reference exception in 102.3c?
            {
                //14.1c: Variable -> Select -Skip -> Table
                if (Pipeline.CurrentIndex >= Pipeline.List.Count - 1)
                    return false;
            }

            //14.1a: Table -> Select -Skip -> Table
            return true;
        }

        private bool NotReadySkipLast()
        {
            if (Pipeline == null)
                return false;

            if (PipeFromVariableWithProgress)
            {
                //15.1c: Variable -> Select -SkipLast -> Table
                var maxCount = EntirePipeline.List.Count - selectObject.TotalAnySkip;

                if (Pipeline.CurrentIndex + 1 == maxCount)
                    return false;

                return true;
            }
            else
            {
                if (previousManager != null)
                {
                    //15.1a: Table -> Select -SkipLast -> Table
                    var total = previousManager.TotalRecords.Value;

                    var maxCount = total - selectObject.TotalAnySkip;

                    if (Pipeline.CurrentIndex + 1 == maxCount)
                        return false;
                }
            }

            return true;
        }

        private bool NotReadyIndex()
        {
            if (PipeFromVariableWithProgress)
            {
                //16.1c: Variable -> Select -Index -> Table
                if (Pipeline.CurrentIndex + 1 == selectObject.Index.Last() + 1)
                    return false;
            }
            else
            {
                //16.1a: Table -> Select -Index -> Table
                if (previousManager.recordsProcessed == selectObject.Index.Last() + 1)
                    return false;
            }

            return true;
        }
    }
}
