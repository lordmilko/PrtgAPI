namespace PrtgAPI
{
    /// <summary>
    /// Represents a uniquely identifiable object within PRTG.
    /// </summary>
    public interface IPrtgObject : IObject
    {
        /// <summary>
        /// Unique identifier of this object within PRTG.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// ID of this object's parent.
        /// </summary>
        int ParentId { get; }
    }
}