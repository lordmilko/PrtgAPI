using System.Diagnostics;
using System.Management.Automation;

namespace PrtgAPI.PowerShell.Progress
{
    /// <summary>
    /// Enhanced PowerShell progress record that also stores the SourceId to be used with the record.
    /// </summary>
    public class ProgressRecordEx : ProgressRecord
    {
        internal const string DefaultActivity = "Activity";
        internal const string DefaultDescription = "Description";

        [DebuggerDisplay("{Completed}")]
        internal class SharedState
        {
            /// <summary>
            /// Indicates whether this record has been completed. If this record was previously completed and is re-written with <see cref="ProgressRecordType.Processing"/>, this value will become false.
            /// </summary>
            public bool Completed { get; set; }
        }

        /// <summary>
        /// Indicates whether progress has been written to this record.
        /// </summary>
        public bool ProgressWritten { get; set; }

        /// <summary>
        /// Indicates whether the cmdlet that owns this record is responsible for completing it.
        /// </summary>
        public bool CmdletOwnsRecord { get; set; } = true;

        internal SharedState State { get; set; } = new SharedState();

        /// <summary>
        /// Indicates whether this progress record is currently in a completed state.
        /// </summary>
        public bool Completed => State.Completed;

        /// <summary>
        /// Indicates that the progress record's <see cref="ProgressRecord.Activity"/> has a value.
        /// </summary>
        public bool HasActivity => Activity != DefaultActivity;

        /// <summary>
        /// Indicates that the progress record's <see cref="ProgressRecord.StatusDescription"/> has a value.
        /// </summary>
        public bool HasDescription => StatusDescription != DefaultDescription;

        /// <summary>
        /// Indicates that both the progress record's <see cref="ProgressRecord.Activity"/> and <see cref="ProgressRecord.StatusDescription"/> have values.
        /// </summary>
        public bool ContainsProgress => HasActivity && HasDescription;

        /// <summary>
        /// Indicates that the progress record's <see cref="ProgressRecord.CurrentOperation"/> has a value.
        /// </summary>
        public bool HasOperation => CurrentOperation != null;

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
        /// Initializes a new instance of the <see cref="ProgressRecordEx"/> class.
        /// </summary>
        /// <param name="activityId">A unique numeric key that identifies the activity to which this record applies.</param>
        /// <param name="sourceId">The internal source ID to associate with this record.</param>
        public ProgressRecordEx(int activityId, long sourceId) : base(activityId, DefaultActivity, DefaultDescription)
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