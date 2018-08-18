using System;

namespace PrtgAPI
{
    /// <summary>
    /// Extension methods for interacting with <see cref="IObject"/> types.
    /// </summary>
    public static class IObjectExtensions
    {
        /// <summary>
        /// Retrieves the Object ID of an unspecified <see cref="IObject"/> type.
        /// </summary>
        /// <param name="obj">The instance to retrieve the Object ID of.</param>
        /// <returns>The object's Object ID.</returns>
        public static int GetId(this IObject obj)
        {
            if (obj is IPrtgObject)
                return ((IPrtgObject) obj).Id;

            if (obj is ISubObject)
                return ((ISubObject) obj).SubId;

            if (obj is IEventObject)
                return ((IEventObject) obj).ObjectId;

            throw new NotSupportedException($"Don't know how to retrieve the object ID for object of type {obj.GetType()}");
        }
    }
}
