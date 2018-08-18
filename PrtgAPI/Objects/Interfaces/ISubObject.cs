namespace PrtgAPI
{
    /// <summary>
    /// Represents an object that may exist one or more times across different <see cref="IPrtgObject"/> instances.
    /// </summary>
    public interface ISubObject : IObject
    {
        /// <summary>
        /// The identifier of this object under its parent object.
        /// </summary>
        int SubId { get; set; }
    }
}