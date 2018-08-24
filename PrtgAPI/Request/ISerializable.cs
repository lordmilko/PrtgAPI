namespace PrtgAPI.Request
{
    /// <summary>
    /// Represents a type whose value when serialized is different from its typical string representation.
    /// </summary>
    internal interface ISerializable
    {
        /// <summary>
        /// Gets the string format of this type for use in serialization requests.
        /// </summary>
        /// <returns></returns>
        string GetSerializedFormat();
    }
}
