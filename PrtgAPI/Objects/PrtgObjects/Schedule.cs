using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Request;
using PrtgAPI.Schedules;

namespace PrtgAPI
{
    /// <summary>
    /// <para type="description">Represents a schedule used to indicate when monitoring should be active on an object.</para>
    /// </summary>
    public class Schedule : PrtgObject, IEquatable<Schedule>, ISerializable
    {
        private string url;

        /// <summary>
        /// URL of this object.
        /// </summary>
        [XmlElement("baselink")]
        [PropertyParameter(Property.Url)]
        public string Url
        {
            get { return Lazy(() => url); }
            set { url = value; }
        }

        /// <summary>
        /// Specifies the times monitoring is active.<para/>
        /// If this object was retrieved from a read only user, this value is null.
        /// </summary>
        public TimeTable TimeTable { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Schedule"/> class.
        /// </summary>
        /// <param name="id">The ID of the schedule.</param>
        /// <param name="name">The name of the schedule.</param>
        public Schedule(int id, string name) : this($"{id}|{name}|")
        {
        }

        internal Schedule()
        {
        }

        internal Schedule(string raw) : base(raw)
        {
        }

        /// <summary>
        /// Returns a boolean indicating if the passed in object obj is
        /// Equal to this. The specified object is equal to this if both
        /// objects have the same raw value.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (other.GetType() != typeof(Schedule))
                return false;

            return IsEqual((Schedule)other);
        }

        /// <summary>
        /// Returns a boolean indicating if the passed in object obj is
        /// Equal to this. The specified object is equal to this if both
        /// objects have the same raw value.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(Schedule other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return IsEqual(other);
        }

        private bool IsEqual(Schedule other)
        {
            return ((ISerializable)this).GetSerializedFormat() == ((ISerializable)other).GetSerializedFormat();
        }

        /// <summary>
        /// Returns a hash code for this object. If two Schedules have
        /// the same raw value, they will have the same hash code.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var result = 0;

                result = (result * 401) ^ ((ISerializable)this).GetSerializedFormat().GetHashCode();

                return result;
            }
        }

        [ExcludeFromCodeCoverage]
        string ISerializable.GetSerializedFormat()
        {
            return $"{Id}|{Name}|";
        }
    }
}
