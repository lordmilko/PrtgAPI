namespace PrtgAPI
{
    /// <summary>
    /// Represents an abstract object of an unspecified <see cref="IObject"/> type capable of
    /// appearing in tables.<para/>This type is not considered a concrete implementation of <see cref="IObject"/>.
    /// Implementing types must also implement a real derivation of <see cref="IObject"/> such as <see cref="IPrtgObject"/>.
    /// </summary>
    public interface ITableObject : IObject
    {
        /// <summary>
        /// The unique identifier of this object or the ID of the object this object pertains to.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// The display type of this object or the type of the object this object pertains to.
        /// </summary>
        string DisplayType { get; }

        /// <summary>
        /// The raw type name of this object or the raw type name of the object this object pertains to..
        /// </summary>
        StringEnum<ObjectType> Type { get; }

        /// <summary>
        /// Tags contained on this object or the object this object pertains to.
        /// </summary>
        string[] Tags { get; }

        /// <summary>
        /// Whether or not the object is currently active (in a monitoring state). If false, the object is paused.
        /// </summary>
        bool Active { get; }
    }
}
