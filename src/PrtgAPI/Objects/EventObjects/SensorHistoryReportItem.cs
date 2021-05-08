using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace PrtgAPI
{
    /// <summary>
    /// Represents a historical occurrence of a <see cref="Sensor"/> being in a particular <see cref="PrtgAPI.Status"/>.
    /// </summary>
    [DebuggerDisplay("SensorId = {SensorId}, Status = {Status}, Duration = {Duration}, StartDate = {StartDate}")]
    public class SensorHistoryReportItem : IEventObject
    {
        [ExcludeFromCodeCoverage]
        string IObject.Name => StartDate.ToString(CultureInfo.InvariantCulture) + " - " + EndDate.ToString(CultureInfo.InvariantCulture);

        [ExcludeFromCodeCoverage]
        int IEventObject.ObjectId => SensorId;

        /// <summary>
        /// Gets the ID of the sensor to which this historical event pertains.
        /// </summary>
        public int SensorId { get; }

        /// <summary>
        /// Gets the status the sensor was within within the <see cref="StartDate"/> and <see cref="EndDate"/>.
        /// </summary>
        public Status Status { get; }

        /// <summary>
        /// Gets the start time when the sensor entered the given <see cref="PrtgAPI.Status"/>.<para/>
        /// Note: if the sensor was already in the specified status prior to the start date of the the sensor history
        /// report that this item was included in,<para/>this value will simply be the beginning of the entire requested reporting period.
        /// </summary>
        public DateTime StartDate { get; }

        /// <summary>
        /// Gets the start time when the sensor exited the given <see cref="PrtgAPI.Status"/>.<para/>
        /// Note: if the sensor continued to be in the specified status after the end date of the the sensor history
        /// report that this item was included in,<para/>this value will simply be the end of the entire request reporting period.
        /// </summary>
        public DateTime EndDate { get; }

        /// <summary>
        /// Gets the duration the sensor was in the specified <see cref="Status"/> based on the <see cref="StartDate"/> and <see cref="EndDate"/> properties of this report item.
        /// </summary>
        public TimeSpan Duration { get; }

        internal SensorHistoryReportItem(int sensorId, Status status, DateTime startDate, DateTime endDate)
        {
            SensorId = sensorId;
            Status = status;
            StartDate = startDate;
            EndDate = endDate;
            Duration = EndDate - StartDate;
        }
    }
}