using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Progress
{
    /// <summary>
    /// Extended progress state that must be saved between <see cref="ProgressManager"/> teardowns with each call to ProcessRecord
    /// </summary>
    class ProgressManagerEx
    {
        internal Pipeline BlockingSelectPipeline { get; set; }

        /// <summary>
        /// The last record that was written to before a <see cref="PrtgMultiOperationCmdlet"/> entered its EndProcessing method.
        /// </summary>
        internal ProgressRecordEx CachedRecord { get; set; }

        internal ProgressRecordEx CurrentRecord { get; set; }
        internal ProgressRecordEx PreviousRecord { get; set; }

        internal long? lastSourceId { get; set; }

        internal bool PipeFromSingleVariable { get; set; }

        private bool? cmdletOwnsRecord;

        internal bool CmdletOwnsRecord
        {
            get { return cmdletOwnsRecord ?? CurrentRecord.CmdletOwnsRecord; }
            set
            {
                cmdletOwnsRecord = value;
                CurrentRecord.CmdletOwnsRecord = value;
            }
        }

        internal void RestoreRecordState()
        {
            if (cmdletOwnsRecord != null)
                CurrentRecord.CmdletOwnsRecord = cmdletOwnsRecord.Value;
        }
    }
}
