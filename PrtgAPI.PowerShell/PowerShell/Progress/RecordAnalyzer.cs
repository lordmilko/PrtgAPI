using System.Linq;
using Microsoft.PowerShell.Commands;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Progress
{
    class MultiOperationRecordAnalyzer
    {
        /// <summary>
        /// Indicates that the next cmdlet in the pipeline is both a <see cref="PrtgMultiOperationCmdlet"/> and is executing with -Batch:$true
        /// </summary>
        public bool PipeToMultiOperation => manager.NextCmdletIsPostProcessMode;

        /// <summary>
        /// Indicates that the current cmdlet is both a <see cref="PrtgMultiOperationCmdlet"/> and is executing with -Batch:$true
        /// </summary>
        public bool IsMultiOperation => manager.PostProcessMode();

        public bool IsPipeFromSelectObject => manager.PreviousCmdletIsSelectObject || IsPipeFromActionFromSelectObject;

        private bool IsPipeFromActionFromSelectObject
        {
            get
            {
                var commands = manager.CacheManager.GetPipelineCommands();

                var myIndex = commands.IndexOf(manager.cmdlet) - 1;

                //Check whether every cmdlet from this cmdlet to the previous Select-Object cmdlet is a PrtgOperationCmdlet
                for (int i = myIndex; i >= 0; i--)
                {
                    //If we found a Select-Object cmdlet, return true
                    if (SelectObjectDescriptor.IsSelectObjectCommand(commands[i]))
                        return true;

                    //If we're not a PrtgOperation cmdlet, return false. Otherwise, we're still in a chain of PrtgOperationCmdlets, so we'll next check the previous cmdlet
                    if (!(commands[i] is PrtgOperationCmdlet))
                        return false;
                }

                return false;
            }
        }

        public bool PipelineStillWriting
        {
            get
            {
                if (manager.PreviousCmdletIsSelectObject)
                    return SelectObjectPipelineStillWriting;

                if (IsPipeFromActionFromSelectObject)
                    return ActionFromSelectObjectPipelineStillWriting;

                return NormalPipelineStillWriting;
            }
        }

        private bool SelectObjectPipelineStillWriting
        {
            get
            {
                return false;
            }
        }

        private bool ActionFromSelectObjectPipelineStillWriting
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Indicates whether any cmdlet up the pipeline is still generating records;
        /// </summary>
        private bool NormalPipelineStillWriting
        {
            get
            {
                if (PreviousVariableStillWriting)
                    return true;

                var cmdlets = manager.CacheManager.GetPipelineCommands();

                var myIndex = cmdlets.IndexOf(manager.cmdlet);

                var prtgCmdlets = cmdlets.Take(myIndex).OfType<PrtgCmdlet>().Where(c => !(c is PrtgOperationCmdlet)).ToList();

                foreach (var cmdlet in prtgCmdlets)
                {
                    var pm = cmdlet.ProgressManager;

                    if (pm.RecordsProcessed > -1 && pm.RecordsProcessed < pm.TotalRecords)
                        return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Indicates whether a variable or a cmdlet is still directly piping records into this cmdlet.
        /// </summary>
        public bool PreviousSourceStillWriting
        {
            get
            {
                if (manager.PreviousCmdletIsSelectObject)
                    return PreviousSelectObjectStillWriting;

                if (IsPipeFromActionFromSelectObject)
                    return PreviousActionFromSelectObjectStillWaiting;

                return PreviousCmdletStillWriting || (PreviousVariableStillWriting && previousCmdlet == null);
            }
        }

        public bool ReceivedLastRecord => ReceivedLastSelectObjectRecord || ReceivedLastNormalCmdletRecord;

        public bool ReceivedLastSelectObjectRecord => manager.readyParser.Ready() && IsPipeFromSelectObject;

        /// <summary>
        /// Indicates whether the current <see cref="PrtgMultiOperationCmdlet"/> has received the last record from the previous record-generating cmdlet, or from the variable pumping records directly into it.
        /// </summary>
        public bool ReceivedLastNormalCmdletRecord
        {
            get
            {
                if (previousCmdlet != null)
                {
                    if (previousCmdlet is PrtgOperationCmdlet)
                    {
                        if (!(previousNonOperationCmdlet is PrtgOperationCmdlet))
                        {
                            if (previousNonOperationCmdlet.ProgressManager.RecordsProcessed == previousNonOperationCmdlet.ProgressManager.TotalRecords)
                                return true;
                        }
                    }

                    if (previousManger.RecordsProcessed == previousManger.TotalRecords)
                        return true;
                }

                if (manager.PipeFromVariableWithProgress)
                {
                    if (manager.EntirePipeline.CurrentIndex == -1)
                        return true;

                    if (manager.EntirePipeline.CurrentIndex + 1 >= manager.EntirePipeline.List.Count)
                        return true;
                }

                return false;
            }
        }

        public bool PipelineFinished => (InputPipelineDestroyed || VariableInPipelineOutOfBounds || PreviousActionFromSelectDestroyed) && IsOnlyPrtgCmdletInPipeline;

        private bool InputPipelineDestroyed => manager.Pipeline == null;

        private bool VariableInPipelineOutOfBounds => manager.PipeFromVariableWithProgress && manager.Pipeline.CurrentIndex == -1; //i think this indicates we're pipe from variable, and in endprocessing

        private bool PreviousActionFromSelectDestroyed
        {
            get
            {
                var firstCmdlet = manager.CacheManager.TryGetFirstOperationCmdletAfterSelectObject();

                if (firstCmdlet == null)
                    return false;

                if (firstCmdlet != manager.cmdlet && IsOnlyPrtgCmdletInPipeline)
                    return true;

                return false;
            }
        }

        private bool IsOnlyPrtgCmdletInPipeline => ProgressManager.progressPipelines.RecordsInCurrentPipeline == 1;

        /// <summary>
        /// Indicates whether the variable that was piped directly into this cmdlet (if applicable) is still piping records to this cmdlet.
        /// </summary>
        public bool PreviousVariableStillWriting
        {
            get
            {
                if (manager.PipeFromVariableWithProgress)
                {
                    if (manager.EntirePipeline.CurrentIndex == -1)
                        return false;

                    if (manager.EntirePipeline.CurrentIndex + 1 < manager.EntirePipeline.List.Count)
                        return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Indicates that the previous cmdlet in the pipeline is still writing records to this cmdlet.
        /// </summary>
        public bool PreviousCmdletStillWriting
        {
            get
            {
                if (previousCmdlet == null)
                    return false;

                if (PipeToMultiOperation)
                {
                    
                }
                else if (IsMultiOperation)
                {
                    if (previousManger.RecordsProcessed == -1)
                    {
                        if (previousCmdlet is PrtgOperationCmdlet)
                        {
                            if (previousNonOperationCmdlet == null)
                                return false;

                            if (previousNonOperationCmdlet is PrtgOperationCmdlet)
                                return false;

                            if (previousNonOperationCmdlet.ProgressManager.RecordsProcessed < previousNonOperationCmdlet.ProgressManager.TotalRecords)
                                return true;
                        }

                        return false;
                    }

                    if (previousManger.RecordsProcessed < previousManger.TotalRecords)
                        return true;
                }

                return false;
            }
        }

        private bool PreviousSelectObjectStillWriting => CalculatePreviousSelectObjectStillWriting(previousManger) || CalculatePreviousSelectObjectFromVariableStillWriting(); //todo: wont this cause issues when piping from a variable, since there is no previous cmdlet

        private bool PreviousActionFromSelectObjectStillWaiting => CalculatePreviousSelectObjectStillWriting(manager.CacheManager.TryGetFirstOperationCmdletAfterSelectObject()?.ProgressManager, previousNonOperationCmdlet?.ProgressManager) || CalculatePreviousSelectObjectFromVariableStillWriting();

        private bool CalculatePreviousSelectObjectStillWriting(ProgressManager previousManager, ProgressManager recordManager = null)
        {
            if (previousManger == null) //We're piping from a variable
                return false;

            if (recordManager == null)
                recordManager = previousManager;

            var selectObject = previousManager.upstreamSelectObjectManager;

            if (selectObject != null)
            {
                if (PreviousCmdletStillWriting)
                {
                    if (selectObject.HasFirst)
                    {
                        if (recordManager.RecordsProcessed < selectObject.First)
                            return true;
                    }
                }
                else
                {

                }
            }

            return false;
        }

        private bool CalculatePreviousSelectObjectFromVariableStillWriting()
        {
            var selectObject = manager.upstreamSelectObjectManager;

            if (selectObject != null)
            {
                if (PreviousVariableStillWriting)
                {
                    if (selectObject.HasFirst)
                    {
                        if (manager.EntirePipeline.CurrentIndex + 1 < selectObject.First)
                            return true;
                    }
                }
            }

            return false;
        }

        private PrtgCmdlet previousCmdlet => manager.CacheManager.GetPreviousPrtgCmdlet();
        private ProgressManager previousManger => previousCmdlet?.ProgressManager;

        private PrtgCmdlet previousNonOperationCmdlet => manager.CacheManager.TryGetPreviousPrtgCmdletOfNotType<PrtgOperationCmdlet>();
        
        private RecordAnalyzer analyzer;
        private ProgressManager manager;

        public MultiOperationRecordAnalyzer(ProgressManager manager)
        {
            analyzer = new RecordAnalyzer(manager);
            this.manager = manager;
        }
    }

    class RecordAnalyzer
    {
        public int TotalRecords { get; }

        public int RecordsProcessed { get; }

        public int PreviousTotalRecords { get; }

        public int PreviousRecordsProcessed { get; }

        private ProgressManager manager;

        public RecordAnalyzer(ProgressManager manager)
        {
            this.manager = manager;
        }
    }
}
