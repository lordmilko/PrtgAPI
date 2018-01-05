namespace PrtgAPI.PowerShell.Progress
{
    interface IProgressWriter
    {
        /// <summary>
        /// Write a new process record.
        /// </summary>
        /// <param name="progressRecord">The progress record to write.</param>
        void WriteProgress(ProgressRecordEx progressRecord);

        /// <summary>
        /// Update a previous progress record, or if the progressRecord.ParentActivityId is specified, make a progress record a child of its parent.
        /// </summary>
        /// <param name="sourceId">The source ID of the previously written record.</param>
        /// <param name="progressRecord">The progress record to write.</param>
        void WriteProgress(long sourceId, ProgressRecordEx progressRecord);
    }
}
