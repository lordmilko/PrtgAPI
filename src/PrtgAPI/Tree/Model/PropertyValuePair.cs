using System;
using System.Diagnostics;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Represents a property and value of an <see cref="IPrtgObject"/>.
    /// </summary>
    [DebuggerDisplay("Property = {Property.ToString(),nq}, Value = {Value,nq}, ParentId = {ParentId}")]
    public class PropertyValuePair : ITreeValue
    {
        string ITreeValue.Name => Property.ToString();

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        public Either<ObjectProperty, string> Property { get; }

        /// <summary>
        /// Gets the value of the property.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Gets the ID of the object that contains this property.
        /// </summary>
        public int ParentId { get; }

        /// <summary>
        /// Gets the ID of the property. This property is not used and will always be null.
        /// </summary>
        public int? Id { get; }

        internal PropertyValuePair(Either<IPrtgObject, int> parentOrId, Either<ObjectProperty, string> property, object value)
        {
            ParentId = parentOrId.GetId();
            Property = property;
            Value = value;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"[{Property}, {Value}]";
        }
    }
}
