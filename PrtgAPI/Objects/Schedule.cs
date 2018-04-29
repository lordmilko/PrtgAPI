using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Helpers;
using PrtgAPI.Objects.Shared;
using PrtgAPI.Request;

namespace PrtgAPI
{
    /// <summary>
    /// <para type="description">Represents a schedule used to indicate when monitoring should be active on an object.</para>
    /// </summary>
    public class Schedule : ObjectTable, IEquatable<Schedule>, IFormattable, ILazy
    {
        private string url;

        /// <summary>
        /// URL of this object.
        /// </summary>
        [XmlElement("baselink")]
        [PropertyParameter(nameof(Property.Url))]
        public string Url
        {
            get { return Lazy(() => url); }
            set { url = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Schedule"/> class.
        /// </summary>
        public Schedule()
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
        [ExcludeFromCodeCoverage]
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
        /// objects same raw value.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        [ExcludeFromCodeCoverage]
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
            return raw == other.raw;
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

                result = (result * 401) ^ raw.GetHashCode();

                return result;
            }
        }

        [ExcludeFromCodeCoverage]
        string IFormattable.GetSerializedFormat()
        {
            return $"{Id}|{Name}|";
        }

        #region ILazy

        Lazy<XDocument> ILazy.LazyXml { get; set; }

        [ExcludeFromCodeCoverage]
        internal Lazy<XDocument> LazyXml
        {
            get { return ((ILazy)this).LazyXml; }
            set { ((ILazy)this).LazyXml = value; }
        }

        object ILazy.LazyLock { get; } = new object();

        bool ILazy.LazyInitialized { get; set; }

        #endregion
    }
}
