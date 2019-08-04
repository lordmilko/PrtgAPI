namespace PrtgAPI
{
    /// <summary>
    /// Represents an event that can occur to a <see cref="IPrtgObject"/>.
    /// </summary>
    public interface IEventObject : IObject
    {
        /// <summary>
        /// ID of the object the event applies to.
        /// </summary>
        int ObjectId { get; }
    }
}
