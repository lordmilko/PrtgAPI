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
            if (selectObject.HasLast)
            {
                return NotReadyFirst_Last();
            }

            if (selectObject.HasSkip)
            {
                return NotReadyFirst_Skip();
            }

            if (selectObject.HasSkipLast)
            {
                if (PipeFromVariableWithProgress)
                {
                    //12.7c: Variable -> Select -First -> Select -SkipLast -> Table
                    if (Pipeline.CurrentIndex + 1 >= Pipeline.List.Count && selectObject.SplitLastOverTwo)
                        return false;

                    return true;
                }
                else
                {
                    //12.3c: Table -> Select -First -> Select -SkipLast -> Table
                    if (Pipeline.CurrentIndex + 1 >= Pipeline.List.Count && selectObject.SplitLastOverTwo)
                        return false;

                    return true;
                }
            }

            if (selectObject.HasIndex)
            {
            }

            if (PipeFromVariableWithProgress)
            {
                //12.1c: Variable -> Select -First -> Table
                if (Pipeline.CurrentIndex + 1 < selectObject.First)
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

        private bool NotReadyFirst_Last()
        {
            if (PipeFromVariableWithProgress)
            {
                //12.6a: Variable -> Select -First -Last -> Table
                if (Pipeline.CurrentIndex >= Pipeline.List.Count - 1)
                    return false;

                return true;
            }
            else
            {
                //12.4a: Table -> Select -First -Last -> Action
                if (Pipeline.CurrentIndex + 1 == TotalRecords)
                    return false;

                //12.2a: Table -> Select -First -Last -> Table
                if (selectObject.First > Pipeline.CurrentIndex + 1)
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
            if (selectObject.HasFirst)
            {
                if (Pipeline.CurrentIndex + 1 <= previousManager.TotalRecords - selectObject.Last)
                    return true;

                return false;
            }

            if (selectObject.HasSkip)
            {
                if (Pipeline.CurrentIndex + 1 < Pipeline.List.Count - selectObject.Skip)
                    return true;

                return false;
            }

            if (selectObject.HasSkipLast)
            {
            }

            if (selectObject.HasIndex)
            {
            }

            //13.1a: Table -> Select -Last -> Table
            if (Pipeline.CurrentIndex + 1 < Pipeline.List.Count)
                return true;

            return false;
        }
        
        private bool NotReadySkip()
        {
            if (selectObject.HasFirst)
            {
                if (PipeFromVariableWithProgress)
                {
                    //14.7a: Variable -> Select -Skip -> Select -First -> Table
                    if (Pipeline.CurrentIndex + 1 >= selectObject.First + selectObject.Skip && selectObject.SplitOverTwo)
                        return false;
                }
                else
                {
                    //14.3a: Table -> Select -Skip -> Select -First -> Table
                    if (previousManager.recordsProcessed >= selectObject.First + selectObject.Skip && selectObject.SplitOverTwo)
                        return false;
                }
            }

            if (selectObject.HasLast)
            {
                //14.3b: Table -> Select -Skip -> Select -Last -> Table
                if (Pipeline.CurrentIndex + 1 >= Pipeline.List.Count)
                    return false;
            }

            if (selectObject.HasSkipLast)
            {
                if (PipeFromVariableWithProgress)
                {
                    //14.7c: Variable -> Select -Skip -> Select -SkipLast -> Table
                    var maxCount = EntirePipeline.List.Count - selectObject.TotalAnySkip;

                    if (Pipeline.CurrentIndex + 1 == maxCount)
                        return false;

                    return true;
                }
                else
                {
                    if (previousManager != null)
                    {
                        //14.3c: Table -> Select -Skip -> Select -SkipLast -> Table
                        var total = previousManager.TotalRecords.Value;

                        var maxCount = total - selectObject.TotalAnySkip;

                        if (Pipeline.CurrentIndex + 1 == maxCount)
                            return false;
                    }
                }
            }

            if (selectObject.HasIndex)
            {
                if (PipeFromVariableWithProgress)
                {
                    //14.7d: Variable -> Select -Skip -> Select -Index -> Table
                    if (Pipeline.CurrentIndex + 1 == selectObject.TotalSkip + selectObject.Index.Last() + 1)
                        return false;
                }
                else
                {
                    if (previousManager.recordsProcessed == selectObject.TotalSkip + selectObject.Index.Last() + 1)
                        return false;
                }
            }

            if (PipeFromVariableWithProgress)
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
            if (selectObject.HasFirst)
            {
            }

            if (selectObject.HasLast)
            {
            }

            if (selectObject.HasSkip)
            {
            }

            if (selectObject.HasIndex)
            {
            }

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
            if (selectObject.HasFirst)
            {
                if (PipeFromVariableWithProgress)
                {
                }
                else
                {
                    //16.2a: Table -> Select -Index -> Select -First -> Table
                    if (previousManager.recordsProcessed == selectObject.Index.Take(selectObject.First).Last() + 1)
                        return false;
                }
            }

            if (selectObject.HasLast)
            {
            }

            if (selectObject.HasSkip)
            {
            }

            if (selectObject.HasSkipLast)
            {
            }

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
