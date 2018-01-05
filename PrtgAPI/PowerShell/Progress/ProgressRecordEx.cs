using System.Management.Automation;

namespace PrtgAPI.PowerShell.Progress
{
    /// <summary>
    /// Enhanced PowerShell progress record that also stores the SourceId to be used with the record.
    /// </summary>
    public class ProgressRecordEx : ProgressRecord
    {
        /// <summary>
        /// The SourceId to be used when writing progress.
        /// </summary>
        public long SourceId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressRecordEx"/> class.
        /// </summary>
        /// <param name="activityId">A unique numeric key that identifies the activity to which this record applies.</param>
        /// <param name="activity">A description of the activity for which progress is being reported.</param>
        /// <param name="statusDescription">A description of the status of the activity.</param>
        /// <param name="sourceId">The internal source ID to associate with this record.</param>
        public ProgressRecordEx(int activityId, string activity, string statusDescription, long sourceId) : base(activityId, activity, statusDescription)
        {
            SourceId = sourceId;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"sourceid = {SourceId} " + base.ToString();
        }
    }
}