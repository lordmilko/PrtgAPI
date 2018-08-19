using System;
using System.Collections.Generic;
using System.Linq;

namespace PrtgAPI.Schedules
{
    /// <summary>
    /// Provides a view of a <see cref="TimeTable"/> hour across all days of the week.
    /// </summary>
    public class TimeSlotRow : IEquatable<TimeSlotRow>, IComparable<TimeSlotRow>, IComparable
    {
        /// <summary>
        /// The display hour this view applies to.
        /// </summary>
        public string Time => Hour.ToString("00:\\0\\0");

        /// <summary>
        /// The hour this view applies to.
        /// </summary>
        public int Hour { get; }

        /// <summary>
        /// Whether monitoring is active on <see cref="DayOfWeek.Monday"/>.
        /// </summary>
        public bool Monday { get; }

        /// <summary>
        /// Whether monitoring is active on <see cref="DayOfWeek.Tuesday"/>.
        /// </summary>
        public bool Tuesday { get; }

        /// <summary>
        /// Whether monitoring is active on <see cref="DayOfWeek.Wednesday"/>.
        /// </summary>
        public bool Wednesday { get; }

        /// <summary>
        /// Whether monitoring is active on <see cref="DayOfWeek.Thursday"/>.
        /// </summary>
        public bool Thursday { get; }

        /// <summary>
        /// Whether monitoring is active on <see cref="DayOfWeek.Friday"/>.
        /// </summary>
        public bool Friday { get; }

        /// <summary>
        /// Whether monitoring is active on <see cref="DayOfWeek.Saturday"/>.
        /// </summary>
        public bool Saturday { get; }

        /// <summary>
        /// Whether monitoring is active on <see cref="DayOfWeek.Sunday"/>.
        /// </summary>
        public bool Sunday { get; }

        internal TimeSlotRow(int hour, bool monday = true, bool tuesday = true, bool wednesday = true, bool thursday = true,
            bool friday = true, bool saturday = true, bool sunday = true)
        {
            Hour = hour;
            Monday = monday;
            Tuesday = tuesday;
            Wednesday = wednesday;
            Thursday = thursday;
            Friday = friday;
            Saturday = saturday;
            Sunday = sunday;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            List<DayOfWeek> days = new List<DayOfWeek>();

            if (Monday)
                days.Add(DayOfWeek.Monday);
            if (Tuesday)
                days.Add(DayOfWeek.Tuesday);
            if (Wednesday)
                days.Add(DayOfWeek.Wednesday);
            if (Thursday)
                days.Add(DayOfWeek.Thursday);
            if (Friday)
                days.Add(DayOfWeek.Friday);
            if (Saturday)
                days.Add(DayOfWeek.Saturday);
            if (Sunday)
                days.Add(DayOfWeek.Sunday);

            return $"{Time} {GetDescription(days)}";
        }

        private string GetDescription(List<DayOfWeek> days)
        {
            var weekdays = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };
            var weekends = new[] { DayOfWeek.Saturday, DayOfWeek.Sunday };

            if (days.Count == 5 && !days.Except(weekdays).Any())
                return "Weekdays";

            if (days.Count == 2 && !days.Except(weekends).Any())
                return "Weekends";

            if (days.Count == 7)
                return "All Days";
            if (days.Count == 0)
                return "Never";

            return string.Join(", ", days);
        }

        #region IEquatable

        /// <summary>
        /// Returns a boolean indicating if the passed in object obj is
        /// Equal to this. The specified object is equal to this if both
        /// objects have the same hour and are active on the same days of the week.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (other.GetType() != typeof(TimeSlotRow))
                return false;

            return IsEqual((TimeSlotRow)other);
        }

        /// <summary>
        /// Returns a boolean indicating if the passed in object obj is
        /// Equal to this. The specified object is equal to this if both
        /// objects have the same hour and are active on the same days of the week.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(TimeSlotRow other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return IsEqual(other);
        }

        private bool IsEqual(TimeSlotRow other)
        {
            if (Hour != other.Hour)
                return false;
            if (Monday != other.Monday)
                return false;
            if (Tuesday != other.Tuesday)
                return false;
            if (Wednesday != other.Wednesday)
                return false;
            if (Thursday != other.Thursday)
                return false;
            if (Friday != other.Friday)
                return false;
            if (Saturday != other.Saturday)
                return false;
            if (Sunday != other.Sunday)
                return false;

            return true;
        }

        /// <summary>
        /// Returns a hash code for this object. If two time slots rows
        /// have the same time, and are active on the same days of the week
        /// they have the same hash code.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var result = 0;

                result = (result * 431) ^ ToString().GetHashCode();

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
            if (!(other is TimeSlotRow) && other != null)
                throw new ArgumentException($"Cannot compare {nameof(TimeSlotRow)} with value of type {other.GetType().Name}.");

            return CompareTo(other as TimeSlotRow);
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that
        /// indicates whether the current instance precedes, follows, or occurs in the same position in the
        /// sort order as the other object.
        /// </summary>
        /// <param name="other">The object to compare with this instance.</param>
        /// <returns>A value that indicates the relative order of the objects being compared.</returns>
        public int CompareTo(TimeSlotRow other)
        {
            if (other == null)
                return 1;

            var hourComparison = Hour.CompareTo(other.Hour);

            if (hourComparison != 0)
                return hourComparison;

            var mondayComparison = Monday.CompareTo(other.Monday);

            if (mondayComparison != 0)
                return mondayComparison;

            var tuesdayComparison = Tuesday.CompareTo(other.Tuesday);

            if (tuesdayComparison != 0)
                return tuesdayComparison;

            var wednesdayComparison = Wednesday.CompareTo(other.Wednesday);

            if (wednesdayComparison != 0)
                return wednesdayComparison;

            var thursdayComparison = Thursday.CompareTo(other.Thursday);

            if (thursdayComparison != 0)
                return thursdayComparison;

            var fridayComparison = Friday.CompareTo(other.Friday);

            if (fridayComparison != 0)
                return fridayComparison;

            var saturdayComparison = Saturday.CompareTo(other.Saturday);

            if (saturdayComparison != 0)
                return saturdayComparison;

            return Sunday.CompareTo(other.Sunday);
        }

        #endregion
    }
}
