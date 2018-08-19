using System;
using System.Diagnostics;
using System.Linq;

namespace PrtgAPI.Schedules
{
    /// <summary>
    /// Represents an individual cell of a <see cref="TimeTable"/>.
    /// </summary>
    [DebuggerDisplay("Hour = {Hour}, Day = {Day}, Active = {Active}")]
    public class TimeSlot : IEquatable<TimeSlot>, IComparable<TimeSlot>, IComparable
    {
        internal static TimeSlot[] Default(DayOfWeek day, Func<int, bool> getActive = null)
        {
            if (getActive == null)
                getActive = i => true;

            return Enumerable.Range(0, 24).Select(v => new TimeSlot(v, day, getActive(v))).ToArray();
        }

        internal TimeSlot(int hour, DayOfWeek day, bool active = true)
        {
            ValidateArgs(hour, day);

            Hour = hour;
            Day = day;
            Active = active;
        }

        internal static void ValidateArgs(int hour, DayOfWeek? day)
        {
            if (hour < 0 || hour > 24)
                throw new ArgumentException("Hour must be between 0-23.", nameof(hour));

            if (day != null)
            {
                var values = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>();

                if (values.All(v => v != day))
                    throw new ArgumentException($"'{day}' is not a valid {typeof(DayOfWeek).FullName}.");
            }
        }

        /// <summary>
        /// The hour this timeslot applies to. Ranges from 0-23.
        /// </summary>
        public int Hour { get; }

        /// <summary>
        /// The day of the week this timeslot applies to.
        /// </summary>
        public DayOfWeek Day { get; }

        /// <summary>
        /// Whether monitoring should be active in the timespan specified by this timeslot.
        /// </summary>
        public bool Active { get; }

        #region IEquatable

        /// <summary>
        /// Returns a boolean indicating if the passed in object obj is
        /// Equal to this. The specified object is equal to this if both
        /// objects have the same day, hour and active state.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (other.GetType() != typeof(TimeSlot))
                return false;

            return IsEqual((TimeSlot)other);
        }

        /// <summary>
        /// Returns a boolean indicating if the passed in object obj is
        /// Equal to this. The specified object is equal to this if both
        /// objects have the same day, hour and active state.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(TimeSlot other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return IsEqual(other);
        }

        private bool IsEqual(TimeSlot other)
        {
            return Hour == other.Hour && Day == other.Day && Active == other.Active;
        }

        /// <summary>
        /// Returns a hash code for this object. If two time slots have
        /// the same day, hour and active state, they have the same hash code.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var result = 0;

                result = (result * 421) ^ $"{Day}_{Hour}_{Active}".GetHashCode();

                return result;
            }
        }

        #endregion
        #region IComparable

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that
        /// indicates whether the current instance precedes, follows, or occurs in the same position in the
        /// sort order as the other object.
        /// </summary>
        /// <param name="other">The object to compare with this instance.</param>
        /// <returns>A value that indicates the relative order of the objects being compared.</returns>
        public int CompareTo(object other)
        {
            if (!(other is TimeSlot) && other != null)
                throw new ArgumentException($"Cannot compare {nameof(TimeSlot)} with value of type {other.GetType().Name}.");

            return CompareTo(other as TimeSlot);
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that
        /// indicates whether the current instance precedes, follows, or occurs in the same position in the
        /// sort order as the other object.
        /// </summary>
        /// <param name="other">The object to compare with this instance.</param>
        /// <returns>A value that indicates the relative order of the objects being compared.</returns>
        public int CompareTo(TimeSlot other)
        {
            if (other == null)
                return 1;

            var dayComparison = CompareDay(other.Day);

            if (dayComparison != 0)
                return dayComparison;

            var hourComparison = Hour.CompareTo(other.Hour);

            if (hourComparison != 0)
                return hourComparison;

            return Active.CompareTo(other.Active);
        }

        private int CompareDay(DayOfWeek other)
        {
            if (other == DayOfWeek.Sunday)
            {
                if (Day == DayOfWeek.Sunday)
                    return 0; //We're the same day
                return -1; //We are less than Sunday (which is day 7 as far as PRTG is concerned)
            }

            if (Day == DayOfWeek.Sunday)
                return 1; //We know we're not comparing against Sunday, since we would have caught that above

            //We're comparing against values in the middle of the week
            return Day.CompareTo(other);
        }

        #endregion
    }
}
