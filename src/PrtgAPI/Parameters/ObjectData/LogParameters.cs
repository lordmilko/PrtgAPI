using System;
using System.Diagnostics.CodeAnalysis;
using PrtgAPI.Request;
using PrtgAPI.Request.Serialization;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgRequestMessage"/> for retrieving <see cref="Log"/> objects.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class LogParameters : TableParameters<Log>, IShallowCloneable<LogParameters>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogParameters"/> class for retrieving logs between two time periods.
        /// </summary>
        /// <param name="objectId">ID of the object to retrieve logs from. If this value is null or 0, logs will be retrieved from the root group.</param>
        /// <param name="startDate">Start date to retrieve logs from. If this value is null, logs will be retrieved from the current date and time.</param>
        /// <param name="endDate">End date to retrieve logs to. If this value is null, logs will be retrieved until the beginning of all logs.</param>
        /// <param name="count">Number of logs to retrieve. Depending on the number of logs stored in the system, specifying a high number may cause the request to timeout.</param>
        /// <param name="status">Log event types to retrieve records for. If no types are specified, all record types will be retrieved.</param>
        public LogParameters(int? objectId, DateTime? startDate, DateTime? endDate, int? count = 500, params LogStatus[] status) : this(objectId)
        {
            Count = count;

            if (status != null && status.Length > 0)
                Status = status;

            if (endDate != null)
                EndDate = endDate;

            if (startDate != null)
                StartDate = startDate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogParameters"/> class for retrieving logs from a standard time period.
        /// </summary>
        /// <param name="objectId">ID of the object to retrieve logs from. If this value is null or 0, logs will be retrieved from the root group.</param>
        /// <param name="recordAge">Time period to retrieve logs from. Logs will be retrieved from the beginning of this period until the current date and time, ordered from newest to oldest.</param>
        /// <param name="count">Number of logs to retrieve. Depending on the number of logs stored in the system, specifying a high number may cause the request to timeout.</param>
        /// <param name="status">Log event types to retrieve records for. If no types are specified, all record types will be retrieved.</param>
        public LogParameters(int? objectId, RecordAge recordAge = PrtgAPI.RecordAge.LastWeek, int? count = 500, params LogStatus[] status) : this(objectId)
        {
            if (recordAge != PrtgAPI.RecordAge.All)
                RecordAge = recordAge;

            Count = count;

            if (status != null && status.Length > 0)
                Status = status;
        }

        internal LogParameters(int? objectId) : base(Content.Logs)
        {
            if (objectId != null)
                ObjectId = objectId;

            //PRTG returns the same object when you start at 0 and 1, resulting in the 499th and 500th
            //object being duplicates. To prevent this, instead of going 0-499, 500-999 we go 1-500, 501-1000
            StartOffset = 1;
        }

        /// <summary>
        /// Gets or sets the ID of the object these parameters should apply to.
        /// </summary>
        public int? ObjectId
        {
            get { return (int?) this[Parameter.Id]; }
            set { this[Parameter.Id] = value; }
        }

        /// <summary>
        /// Gets or sets the start date to retrieve logs from. If this value is null, logs will be retrieved from the current date and time.
        /// </summary>
        public DateTime? StartDate
        {
            get { return GetDateTime(Property.EndDate); }
            set { SetDateTime(Property.EndDate, value); }
        }

        /// <summary>
        /// Gets or sets the end date to retrieve logs to. If this value is null, logs will be retrieved until the beginning of all logs.
        /// </summary>
        public DateTime? EndDate
        {
            get { return GetDateTime(Property.StartDate); }
            set { SetDateTime(Property.StartDate, value); }
        }

        /// <summary>
        /// Gets or sets log event types to retrieve records for. If no types are specified, all record types will be retrieved.
        /// </summary>
        public LogStatus[] Status
        {
            get { return GetMultiParameterFilterValue<LogStatus>(Property.Status); }
            set { SetMultiParameterFilterValue(Property.Status, value); }
        }

        /// <summary>
        /// Gets or sets the time period to retrieve logs from. Logs will be retrieved from the beginning of this period until the current date and time, ordered from newest to oldest.
        /// </summary>
        public RecordAge? RecordAge
        {
            get { return (RecordAge?) GetFilterValue(Property.RecordAge); }
            set
            {
                if (value != null && value == PrtgAPI.RecordAge.All)
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

            if (val is DateTime)
                return (DateTime?) val;

            //We're retrieving a DateTime we serialized previously
            return TypeHelpers.StringToDate(val.ToString());
        }

        private void SetDateTime(Property property, DateTime? value)
        {
            SetFilterValue(property, value == null ? null : TypeHelpers.DateToString(value.Value));
        }

        LogParameters IShallowCloneable<LogParameters>.ShallowClone()
        {
            var newParameters = new LogParameters(null);

            ShallowClone(newParameters);

            return newParameters;
        }

        object IShallowCloneable.ShallowClone() => ((IShallowCloneable<LogParameters>)this).ShallowClone();
    }
}
