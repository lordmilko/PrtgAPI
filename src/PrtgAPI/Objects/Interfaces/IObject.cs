namespace PrtgAPI
{
    /// <summary>
    /// Represents an abstract object that can exist within PRTG.
    /// </summary>
    public interface IObject
    {
        /// <summary>
        /// The name of this object or the object this object pertains to.
        /// </summary>
        string Name { get; }
    }
}