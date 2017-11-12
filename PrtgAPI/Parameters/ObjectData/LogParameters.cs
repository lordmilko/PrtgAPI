using System;
using System.Diagnostics.CodeAnalysis;
using PrtgAPI.Helpers;
using PrtgAPI.Request;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgUrl"/> for retrieving <see cref="Log"/> objects.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class LogParameters : TableParameters<Log>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogParameters"/> class for retrieving logs between two time periods.
        /// </summary>
        /// <param name="objectId">ID of the object to retrieve logs from. If this value is null or 0, logs will be retrieved from the root group.</param>
        /// <param name="startDate">Start date to retrieve logs from. If this value is null, logs will be from the current date and time.</param>
        /// <param name="endDate">End date to retrieve logs to. If this value is null, logs will be retrieved until the beginning of all logs.</param>
        /// <param name="count">Number of logs to retrieve. Depending on the number of logs stored in the system, specifying a high number may cause the request to timeout.</param>
        /// <param name="status">Log event types to retrieve records for. If no types are specified, all record types will be retrieved.</param>
        public LogParameters(int? objectId, DateTime? startDate, DateTime? endDate, int count = 50, params LogStatus[] status) : this(objectId)
        {
            if (objectId != null)
                this[Parameter.Id] = objectId;

            Count = count;

            if (status != null && status.Length > 0)
                Status = status;

            if (startDate != null)
                StartDate = startDate;

            if (endDate != null)
                EndDate = endDate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogParameters"/> class for retrieving logs from a standard time period.
        /// </summary>
        /// <param name="objectId">ID of the object to retrieve logs from. If this value is null or 0, logs will be retrieved from the root group.</param>
        /// <param name="timeSpan">Time period to retrieve logs from. Logs will be retrieved from the beginning of this period until the current date and time, ordered from newest to oldest.</param>
        /// <param name="count">Number of logs to retrieve. Depending on the number of logs stored in the system, specifying a high number may cause the request to timeout.</param>
        /// <param name="status">Log event types to retrieve records for. If no types are specified, all record types will be retrieved.</param>
        public LogParameters(int? objectId, RecordAge timeSpan = PrtgAPI.RecordAge.LastWeek, int count = 50, params LogStatus[] status) : this(objectId)
        {
            if (timeSpan != PrtgAPI.RecordAge.AllTime)
                RecordAge = timeSpan;

            Count = count;

            if (status != null && status.Length > 0)
                Status = status;
        }

        internal LogParameters(int? objectId) : base(Content.Messages)
        {
            if (objectId != null)
                this[Parameter.Id] = objectId;
        }

        /// <summary>
        /// Start date to retrieve logs from. If this value is null, logs will be retrieved from the current date and time.
        /// </summary>
        public DateTime? StartDate
        {
            get { return GetDateTime(Property.EndDate); }
            set { SetDateTime(Property.EndDate, value); }
        }

        /// <summary>
        /// End date to retrieve logs to. If this value is null, logs will be retrieved until the beginning of all logs.
        /// </summary>
        public DateTime? EndDate
        {
            get { return GetDateTime(Property.StartDate); }
            set { SetDateTime(Property.StartDate, value); }
        }

        /// <summary>
        /// Log event types to retrieve records for. If no types are specified, all record types will be retrieved.
        /// </summary>
        public LogStatus[] Status
        {
            get { return (LogStatus[]) GetFilterValue(Property.Status); }
            set { SetFilterValue(Property.Status, value); }
        }

        /// <summary>
        /// Time period to retrieve logs from. Logs will be retrieved from the beginning of this period until the current date and time, ordered from newest to oldest.
        /// </summary>
        public RecordAge? RecordAge
        {
            get { return (RecordAge?) GetFilterValue(Property.RecordAge); }
            set
            {
                if (value != null && value == PrtgAPI.RecordAge.AllTime)
                {
                    SetFilterValue(Property.RecordAge, null);
                }
                else
                    SetFilterValue(Property.RecordAge, value);
            }
        }

        private DateTime? GetDateTime(Property property)
        {
            var val = GetFilterValue(property);

            if (val == null)
                return null;

            return ParameterHelpers.StringToDate(val.ToString());
        }

        private void SetDateTime(Property property, DateTime? value)
        {
            SetFilterValue(property, value == null ? null : ParameterHelpers.DateToString(value.Value));
        }
    }
}
