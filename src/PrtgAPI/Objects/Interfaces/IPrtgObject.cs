using PrtgAPI.Tree;

namespace PrtgAPI
{
    /// <summary>
    /// Represents a uniquely identifiable object within PRTG.
    /// </summary>
    public interface IPrtgObject : IObject, ITreeValue
    {
        //Redefine Name to consolidate IObject and ITreeValue Name
        //Redefine Id and ParentId to make it clear these are the defining components of an IPrtgObject

        /// <summary>
        /// The name of this object or the object this object pertains to.
        /// </summary>
        new string Name { get; }

        /// <summary>
        /// Unique identifier of this object within PRTG.
        /// </summary>
        new int Id { get; }

        /// <summary>
        /// ID of this object's parent.
        /// </summary>
        new int ParentId { get; }

        /// <summary>
        /// Tags contained on this object.
        /// </summary>
        string[] Tags { get; }

        /// <summary>
        /// The display type of this object. Certain objects may simply report their <see cref="BaseType"/>, while others may get more specific (e.g. a sensor of type "Ping").
        /// </summary>
        string DisplayType { get; }

        /// <summary>
        /// The type of this object.
        /// </summary>
        StringEnum<ObjectType> Type { get; }

        /// <summary>
        /// Whether or not the object is currently active (such as in a monitoring state). If false, the object is paused.
        /// </summary>
        bool Active { get; }
    }
}