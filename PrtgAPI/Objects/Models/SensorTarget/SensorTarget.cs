using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using PrtgAPI.Parameters.Helpers;
using PrtgAPI.Request;
using PrtgAPI.Utilities;

namespace PrtgAPI.Targets
{
    /// <summary>
    /// <para type="description">Represents a resource that can be monitored or used for monitoring by a PRTG Sensor.</para>
    /// </summary>
    /// <typeparam name="T">The type of this object.</typeparam>
    public abstract class SensorTarget<T> : ISerializable, IEquatable<T> where T : SensorTarget<T>
    {
        /// <summary>
        /// Gets the name of the target.
        /// </summary>
        public string Name { get; internal set; }

        private readonly string raw;

        /// <summary>
        /// Gets the individual components of the target's raw value. This field is read-only.
        /// </summary>
        protected readonly string[] components;

        /// <summary>
        /// Initializes a new instance of the <see cref="SensorTarget{T}"/> class.
        /// </summary>
        /// <param name="raw">The raw value of this object.</param>
        protected SensorTarget(string raw)
        {
            if (raw == null)
                throw new ArgumentNullException(nameof(raw));

            this.raw = raw;
            components = raw.Split('|');
            Name = components[0];
        }

        /// <summary>
        /// Converts a string to the raw value used by a dropdown list option in PRTG.
        /// </summary>
        /// <param name="name">The string to encode.</param>
        /// <returns>The encoded format used by PRTG dropdown options.</returns>
        protected static string ToDropDownOption(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value must not be an empty string or whitespace.", nameof(name));

            return $"{name}|{name}||";
        }

        string ISerializable.GetSerializedFormat()
        {
            return raw;
        }

        [ExcludeFromCodeCoverage]
        internal static T ParseStringCompatible(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value is T)
                return (T)value;

            if (value is string)
            {
                if ((string)value == string.Empty)
                    goto Throw;

                if (!((string)value).Contains("|"))
                    value = ToDropDownOption(value.ToString());

                return (T)Activator.CreateInstance(typeof(T), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, new[] { value }, null);
            }

        Throw:
            throw new ArgumentException($"Cannot convert value '{value}' of type '{value.GetType().FullName}' to type '{typeof(T).FullName}'. Value type must be convertable to type {typeof(T).FullName}.", nameof(value));
        }

        internal static List<T> CreateFromDropDownOptions(string response, ObjectProperty name, Func<string, T> createObj)
        {
            var val = ObjectPropertyParser.GetObjectPropertyName(name).TrimEnd('_');

            return CreateFromDropDownOptions(response, val, createObj);
        }

        /// <summary>
        /// Retrieves a list of sensor targets from a list of options on a specified dropdown list.
        /// </summary>
        /// <param name="response">The raw HTML response to parse.</param>
        /// <param name="name">The name of the dropdown list to parse.</param>
        /// <param name="createObj">A function used to construct a sensor target from the target's raw value.</param>
        /// <returns>A list of sensor targets of type T</returns>
        protected static List<T> CreateFromDropDownOptions(string response, string name, Func<string, T> createObj)
        {
            var files = HtmlParser.Default.GetDropDownList(response)
                .Where(d => d.Name == name)
                .SelectMany(d => d.Options
                .Select(o => createObj(o.Value)))
                .ToList();

            return files;
        }

        internal static List<T> CreateFromCheckbox(string response, Parameter name, Func<string, T> createObj)
        {
            var val = name.GetDescription();

            return CreateFromCheckbox(response, val, createObj);
        }

        /// <summary>
        /// Retrieves a list of sensor targets from a list of checkboxes in a specified checkbox group.
        /// </summary>
        /// <param name="response">The raw HTML response to parse.</param>
        /// <param name="name">The name of the checkbox group to parse.</param>
        /// <param name="createObj">A function used to construct a sensor target from the target's raw value.</param>
        /// <returns>A list of sensor targets of type T</returns>
        protected static List<T> CreateFromCheckbox(string response, string name, Func<string, T> createObj)
        {
            var files = HtmlParser.Default.GetInput(response)
                .Where(i => i.Type == Html.InputType.Checkbox && i.Name == name)
                .Select(i => createObj(i.Value))
                .ToList();

            return files;
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

            if (other.GetType() != typeof(T))
                return false;

            return IsEqual((T) other);
        }

        /// <summary>
        /// Returns a boolean indicating if the passed in object obj is
        /// Equal to this. The specified object is equal to this if both
        /// objects are of the same type and have the same <see cref="raw"/> value.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        [ExcludeFromCodeCoverage]
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
