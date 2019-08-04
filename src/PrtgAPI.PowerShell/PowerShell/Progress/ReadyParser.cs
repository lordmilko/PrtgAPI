using System;
using System.Linq;

namespace PrtgAPI.PowerShell.Progress
{
    class ReadyParser
    {
        private SelectObjectManager selectObject;

        private ProgressManager manager;
        private ProgressManager previousManager;

        public Pipeline Pipeline => manager.Pipeline;

        public int? TotalRecords => manager.TotalRecords;
        public bool PipeFromVariableWithProgress => manager.PipeFromVariableWithProgress;
        public Pipeline EntirePipeline => manager.EntirePipeline;

        public ReadyParser(ProgressManager manager)
        {
            this.manager = manager;

            selectObject = manager.upstreamSelectObjectManager;

            var previousCmdlet = this.manager.CacheManager.GetPreviousPrtgCmdlet();
            previousManager = previousCmdlet?.ProgressManager;
        }

        public bool Ready()
        {
            if (manager.UnsupportedSelectObjectProgress)
                return true;

            if (!manager.PreviousCmdletIsSelectObject)
                return true;

            var firstCmdlet = manager.upstreamSelectObjectManager.Commands.Last();

            if (firstCmdlet.HasFirst)
                return ReadyFirst();
            if (firstCmdlet.HasLast)
                return ReadyLast();
            if (firstCmdlet.HasSkip)
                return ReadySkip();
            if (firstCmdlet.HasSkipLast)
                return ReadySkipLast();
            if (firstCmdlet.HasIndex)
                return ReadyIndex();

            return true;
        }

        private bool ReadyFirst()
        {
            if (PipeFromVariableWithProgress)
            {
                //12.1c: Variable -> Select -First -> Table
                if (EntirePipeline.CurrentIndex + 1 < selectObject.First)
                    return false;
            }
            else
            {
                if (previousManager.RecordsProcessed == -1)
                    throw new NotImplementedException("Attempted to process a recordsProcessed that is not in use.");

                //12.1a: Get-Probe -Count 3 | Select -First 2 | Get-Device
                if (previousManager.RecordsProcessed < selectObject.First)
                    return false;
            }

            return true;
        }

        private bool ReadyLast()
        {
            if (Pipeline == null)
                return true;

            //13.1a: Table -> Select -Last -> Table
            if (Pipeline.CurrentIndex + 1 < Pipeline.List.Count)
                return false;

            return true;
        }
        
        private bool ReadySkip()
        {
            if (PipeFromVariableWithProgress) //todo: why isnt this causing a null reference exception in 102.3c?
            {
                //14.1c: Variable -> Select -Skip -> Table
                if (Pipeline.CurrentIndex >= Pipeline.List.Count - 1)
                    return true;
            }

            //14.1a: Table -> Select -Skip -> Table
            return false;
        }

        private bool ReadySkipLast()
        {
            if (Pipeline == null)
                return true;

            if (PipeFromVariableWithProgress)
            {
                //15.1c: Variable -> Select -SkipLast -> Table
                var maxCount = EntirePipeline.List.Count - selectObject.TotalAnySkip;

                if (Pipeline.CurrentIndex + 1 == maxCount)
                    return true;

                return false;
            }
            else
            {
                if (previousManager != null)
                {
                    //15.1a: Table -> Select -SkipLast -> Table
                    var total = previousManager.TotalRecords.Value;

                    var maxCount = total - selectObject.TotalAnySkip;

                    if (Pipeline.CurrentIndex + 1 == maxCount)
                        return true;
                }
            }

            return false;
        }

        private bool ReadyIndex()
        {
            if (PipeFromVariableWithProgress)
            {
                //16.1c: Variable -> Select -Index -> Table
                if (Pipeline.CurrentIndex + 1 == selectObject.Index.Last() + 1)
                    return true;
            }
            else
            {
                //16.1a: Table -> Select -Index -> Table
                if (previousManager.RecordsProcessed == selectObject.Index.Last() + 1)
                    return true;
            }

            return false;
        }
    }
}
