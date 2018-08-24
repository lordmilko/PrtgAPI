using System;
using PrtgAPI.Request;

namespace PrtgAPI
{
    /// <summary>
    /// <para type="description">Represents a device template that can be used for performing an auto-discovery.</para>
    /// </summary>
    public class DeviceTemplate : ISerializable, IEquatable<DeviceTemplate>
    {
        /// <summary>
        /// Gets the name of the template.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the config file that defines the device template. Found under C:\Program Files (x86)\PRTG Network Monitor\devicetemplates on the PRTG Core Server.
        /// </summary>
        public string Value { get; }

        internal readonly string raw;

        internal DeviceTemplate(string raw, Func<DeviceTemplate, string> serializedFormat = null)
        {
            var split = raw.Split('|');
            Value = split[0];
            Name = split[1];

            this.serializedFormat = serializedFormat;

            this.raw = raw;
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
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (other.GetType() != typeof(DeviceTemplate))
                return false;

            return IsEqual((DeviceTemplate) other);
        }

        /// <summary>
        /// Returns a boolean indicating if the passed in object obj is
        /// Equal to this. The specified object is equal to this if both
        /// objects are of the same type and have the same <see cref="raw"/> value.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(DeviceTemplate other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return IsEqual(other);
        }

        private bool IsEqual(DeviceTemplate other)
        {
            return raw == other.raw;
        }

        /// <summary>
        /// Returns a hash code for this object. If two Device Templates have the
        /// same <see cref="raw"/> value, they will have the same hash code.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var result = 0;

                result = (result * 439) ^ raw.GetHashCode();

                return result;
            }
        }

        private Func<DeviceTemplate, string> serializedFormat;

        string ISerializable.GetSerializedFormat()
        {
            if (serializedFormat == null)
                return raw;

            return serializedFormat(this);
        }
    }
}
