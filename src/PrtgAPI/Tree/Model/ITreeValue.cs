namespace PrtgAPI.Tree
{
    /// <summary>
    /// Represents a value that is stored in the PRTG Object Tree, such as an <see cref="IPrtgObject"/> or <see cref="ISubObject"/>.
    /// </summary>
    public interface ITreeValue
    {
        /// <summary>
        /// The name of this object.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The ID of this object's parent <see cref="IPrtgObject"/>.
        /// </summary>
        int ParentId { get; }

        /// <summary>
        /// The ID of this object.
        /// </summary>
        int? Id { get; }
    }
}
