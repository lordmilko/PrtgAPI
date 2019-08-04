using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace PrtgAPI.Schedules
{
    /// <summary>
    /// Describes the times a <see cref="Schedule"/> is active. When a schedule is inactive, all objects objects affected by it are <see cref="Status.PausedBySchedule"/>.
    /// </summary>
    public class TimeTable : IEnumerable<TimeSlotRow>, IEquatable<TimeTable>
    {
        /// <summary>
        /// Represents the individual cells and provides a day-centric view of the timetable.
        /// </summary>
        public IReadOnlyDictionary<DayOfWeek, TimeSlot[]> Grid { get; }

        /// <summary>
        /// Provides an hour-centric view of the timetable.
        /// </summary>
        public List<TimeSlotRow> Rows { get; }

        internal TimeTable(List<Html.Input> inputs)
        {
            Grid = new ReadOnlyDictionary<DayOfWeek, TimeSlot[]>(InitializeSlots(inputs));
            Rows = InitializeRows();
        }

        internal TimeTable(TimeSlot[] monday = null, TimeSlot[] tuesday = null, TimeSlot[] wednesday = null, TimeSlot[] thursday = null,
            TimeSlot[] friday = null, TimeSlot[] saturday = null, TimeSlot[] sunday = null)
        {
            monday = ValidateSlots(monday, DayOfWeek.Monday, nameof(monday));
            tuesday = ValidateSlots(tuesday, DayOfWeek.Tuesday, nameof(tuesday));
            wednesday = ValidateSlots(wednesday, DayOfWeek.Wednesday, nameof(wednesday));
            thursday = ValidateSlots(thursday, DayOfWeek.Thursday, nameof(thursday));
            friday = ValidateSlots(friday, DayOfWeek.Friday, nameof(friday));
            saturday = ValidateSlots(saturday, DayOfWeek.Saturday, nameof(saturday));
            sunday = ValidateSlots(sunday, DayOfWeek.Sunday, nameof(sunday));

            var dictionary = new Dictionary<DayOfWeek, TimeSlot[]>
            {
                [DayOfWeek.Monday] = monday,
                [DayOfWeek.Tuesday] = tuesday,
                [DayOfWeek.Wednesday] = wednesday,
                [DayOfWeek.Thursday] = thursday,
                [DayOfWeek.Friday] = friday,
                [DayOfWeek.Saturday] = saturday,
                [DayOfWeek.Sunday] = sunday
            };

            Grid = new ReadOnlyDictionary<DayOfWeek, TimeSlot[]>(dictionary);
            Rows = InitializeRows();
        }

        private TimeSlot[] ValidateSlots(TimeSlot[] slots, DayOfWeek day, string paramName)
        {
            if (slots == null)
                slots = TimeSlot.Default(day);

            if (slots.Length != 24)
                throw new ArgumentException($"Must specify 24 records however only {slots.Length} were specified.", paramName);

            return slots;
        }

        private Dictionary<DayOfWeek, TimeSlot[]> InitializeSlots(List<Html.Input> inputs)
        {
            var items = inputs.Select(i => new
            {
                Value = Convert.ToInt32(i.Value),
                Active = i.Checked
            }).OrderBy(i => i.Value).ToList();

            var values = new[]
            {
                DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday,
                DayOfWeek.Saturday, DayOfWeek.Sunday
            };

            var dayIndex = -1;
            var hourIndex = -1;

            var dictionary = new Dictionary<DayOfWeek, TimeSlot[]>();

            foreach (var item in items)
            {
                var result = item.Value % 24;

                if (result == 0)
                {
                    dayIndex++;
                    hourIndex = 0;
                    dictionary[values[dayIndex]] = new TimeSlot[24];
                }
                else
                    hourIndex++;

                var day = values[dayIndex];

                dictionary[day][hourIndex] = new TimeSlot(hourIndex, day, item.Active);
            }

            return dictionary;
        }

        private List<TimeSlotRow> InitializeRows()
        {
            var objs = new List<TimeSlotRow>();

            for (var i = 0; i < 24; i++)
            {
                var active = Grid.Select(r => r.Value[i].Active).ToList();
                var row = new TimeSlotRow(
                    i,         //Hour
                    active[0], //Monday
                    active[1], //Tuesday
                    active[2], //Wednesday
                    active[3], //Thursday
                    active[4], //Friday
                    active[5], //Saturday
                    active[6]  //Sunday
                );

                objs.Add(row);
            }

            return objs;
        }

        #region Indexers

        /// <summary>
        /// Retrieves all time slots associated with a particular hour across the entire week.
        /// </summary>
        /// <param name="hour">The hour to retrieve time slots for. Must be a value between 0-23.</param>
        /// <returns>All time slots for the specified hour across the entire week.</returns>
        public TimeSlot[] this[int hour]
        {
            get
            {
                TimeSlot.ValidateArgs(hour, null);

                return Grid.Select(s => s.Value[hour]).ToArray();
            }
        }

        /// <summary>
        /// Retrieves all time slots associated with a particular time of day across the entire week.
        /// </summary>
        /// <param name="time">The time of day. Must have a <see cref="TimeSpan.TotalHours"/> less than 24.</param>
        /// <returns>All time slots for the specified time of day across the entire week.</returns>
        public TimeSlot[] this[TimeSpan time] => this[(int)time.TotalHours];

        /// <summary>
        /// Retrieves all time slots present in a single 24-hour day.
        /// </summary>
        /// <param name="day">The day to retrieve time slots for.</param>
        /// <returns>All time slots present in a single day.</returns>
        public TimeSlot[] this[DayOfWeek day] => Grid[day];

        /// <summary>
        /// Retrieves the time slot of a specified hour and day of the week.
        /// </summary>
        /// <param name="hour">The hour to retrieve. Must be a value between 0-23.</param>
        /// <param name="day">The day to retrieve.</param>
        /// <returns>The time slot associated with the specified hour and day.</returns>
        public TimeSlot this[int hour, DayOfWeek day]
        {
            get
            {
                TimeSlot.ValidateArgs(hour, day);

                return Grid[day][hour];
            }
        }

        /// <summary>
        /// Retrieves the time slot of a specified hour and day of the week.
        /// </summary>
        /// <param name="time">The time of day. Must have a <see cref="TimeSpan.TotalHours"/> less than 24.</param>
        /// <param name="day">The day to retrieve.</param>
        /// <returns>The time slot associated with the specified hour and day.</returns>
        public TimeSlot this[TimeSpan time, DayOfWeek day] => this[(int)time.TotalHours, day];

        #endregion
        #region IEnumerable

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<TimeSlotRow> GetEnumerator()
        {
            return Rows.GetEnumerator();
        }

        [ExcludeFromCodeCoverage]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
        #region IEquatable

        /// <summary>
        /// Returns a boolean indicating if the passed in object obj is
        /// Equal to this. The specified object is equal to this if both
        /// objects are timetables that are active at the same times of the week.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (other.GetType() != typeof(TimeTable))
                return false;

            return IsEqual((TimeTable)other);
        }

        /// <summary>
        /// Returns a boolean indicating if the passed in object obj is
        /// Equal to this. The specified object is equal to this if both
        /// objects are timetables that are active at the same times of the week.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(TimeTable other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return IsEqual(other);
        }

        private bool IsEqual(TimeTable other)
        {
            for (var i = 0; i < 24; i++)
            {
                if (Rows[i].Monday != other.Rows[i].Monday)
                    return false;
                if (Rows[i].Tuesday != other.Rows[i].Tuesday)
                    return false;
                if (Rows[i].Wednesday != other.Rows[i].Wednesday)
                    return false;
                if (Rows[i].Thursday != other.Rows[i].Thursday)
                    return false;
                if (Rows[i].Friday != other.Rows[i].Friday)
                    return false;
                if (Rows[i].Saturday != other.Rows[i].Saturday)
                    return false;
                if (Rows[i].Sunday != other.Rows[i].Sunday)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns a hash code for this object. If two timetables are
        /// active at the same times of the week they will have the same hash code.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var builder = new StringBuilder();

                foreach (var row in Rows)
                    builder.Append(row);

                var result = 0;

                result = (result * 433) ^ builder.ToString().GetHashCode();

                return result;
            }
        }

        #endregion
    }
}
