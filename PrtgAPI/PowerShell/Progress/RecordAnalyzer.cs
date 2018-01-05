using System.Linq;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Progress
{
    class MultiOperationRecordAnalyzer
    {
        /// <summary>
        /// Indicates that the next cmdlet in the pipeline is both a <see cref="PrtgMultiOperationCmdlet"/> and is executing with -Batch:$true
        /// </summary>
        public bool PipeToMultiOperation => manager.NextCmdletIsMultiOperationBatchMode;

        /// <summary>
        /// Indicates that the current cmdlet is both a <see cref="PrtgMultiOperationCmdlet"/> and is executing with -Batch:$true
        /// </summary>
        public bool IsMultiOperation => manager.MultiOperationBatchMode();

        /// <summary>
        /// Indicates whether any cmdlet up the pipeline is still generating records;
        /// </summary>
        public bool PipelineStillWriting
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

                    if (pm.recordsProcessed > -1 && pm.recordsProcessed < pm.TotalRecords)
                        return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Indicates whether a variable or a cmdlet is still directly piping records into this cmdlet.
        /// </summary>
        public bool PreviousSourceStillWriting => PreviousCmdletStillWriting || (PreviousVariableStillWriting && previousCmdlet == null);

        /// <summary>
        /// Indicates whether the current <see cref="PrtgMultiOperationCmdlet"/> has received the last record from the previous record-generating cmdlet, or from the variable pumping records directly into it.
        /// </summary>
        public bool ReceivedLastRecord
        {
            get
            {
                if (previousCmdlet != null)
                {
                    if (previousCmdlet is PrtgOperationCmdlet)
                    {
                        if (!(previousNonOperationCmdlet is PrtgOperationCmdlet))
                        {
                            if (previousNonOperationCmdlet.ProgressManager.recordsProcessed == previousNonOperationCmdlet.ProgressManager.TotalRecords)
                                return true;
                        }
                    }

                    if (previousManger.recordsProcessed == previousManger.TotalRecords)
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

        public bool PipelineFinished => (manager.Pipeline == null || (manager.PipeFromVariableWithProgress && manager.Pipeline.CurrentIndex == -1)) && ProgressManager.progressPipelines.RecordsInCurrentPipeline == 1;

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
                    if (previousManger.recordsProcessed == -1)
                    {
                        if (previousCmdlet is PrtgOperationCmdlet)
                        {
                            if (previousNonOperationCmdlet is PrtgOperationCmdlet)
                                return false;

                            if (previousNonOperationCmdlet.ProgressManager.recordsProcessed < previousNonOperationCmdlet.ProgressManager.TotalRecords)
                                return true;
                        }

                        return false;
                    }

                    if (previousManger.recordsProcessed < previousManger.TotalRecords)
                        return true;
                }

                return false;
            }
        }

        private PrtgCmdlet previousCmdlet => manager.CacheManager.GetPreviousPrtgCmdlet();
        private ProgressManager previousManger => previousCmdlet.ProgressManager;

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
