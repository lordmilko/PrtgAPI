using System;

namespace PrtgAPI
{
    /// <summary>
    /// Represents a resource that can be monitored or used for monitoring by a PRTG Sensor.
    /// </summary>
    /// /// <typeparam name="T">The type of this object.</typeparam>
    public abstract class SensorTarget<T> : IFormattable, IEquatable<T> where T : SensorTarget<T>
    {
        /// <summary>
        /// The name of the target.
        /// </summary>
        public string Name { get; internal set; }

        private readonly string raw;

        /// <summary>
        /// The individual components of the target's raw value.
        /// </summary>
        protected readonly string[] components;

        /// <summary>
        /// Initializes a new instance of the <see cref="SensorTarget{T}"/> class.
        /// </summary>
        /// <param name="raw">The raw value of this object.</param>
        protected SensorTarget(string raw)
        {
            this.raw = raw;
            components = raw.Split('|');
        }

        string IFormattable.GetSerializedFormat()
        {
            return raw;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Returns a boolean indicating if the passed in object obj is
        /// Equal to this. The specified object is equal to this if both
        /// objects are of the same type and have the same <see cref="raw"/> value.
        /// </summary>
        /// <param name="other">The object to compare with the current object..</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (other.GetType() != typeof(T))
                return false;

            return IsEqual((T) other);
        }

        /// <summary>
        /// Returns a boolean indicating if the passed in object obj is
        /// Equal to this. The specified object is equal to this if both
        /// objects are of the same type and have the same <see cref="raw"/> value.
        /// </summary>
        /// <param name="other">The object to compare with the current object..</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(T other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return IsEqual(other);
        }

        private bool IsEqual(T other)
        {
            return raw == other.raw;
        }

        /// <summary>
        /// Returns a hash code for this object. If two Sensor Targets are of the same
        /// type and have the same <see cref="raw"/> value, they will have the same hash code.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return $"{typeof(T).AssemblyQualifiedName}_{raw}".GetHashCode();
        }
    }
}
