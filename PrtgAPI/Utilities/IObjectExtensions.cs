using System;
using System.ComponentModel;
using PrtgAPI.Reflection;

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

            throw new NotSupportedException($"Don't know how to retrieve the object ID for object of type {obj.GetType()}.");
        }

        internal static string GetTypeDescription(this IObject value)
        {
            var type = value.GetType();

            if (type == typeof(PrtgObject))
                return ((PrtgObject)value).Type.StringValue;

            return GetTypeDescription(type);
        }

        /// <summary>
        /// Retrieves the value of a <see cref="DescriptionAttribute"/> of the specified type. If the type does not have a <see cref="DescriptionAttribute"/>, its name is used instead.
        /// </summary>
        /// <param name="type">The type whose description should be retrieved.</param>
        /// <returns>The type's name or description.</returns>
        internal static string GetTypeDescription(Type type)
        {
            var attribute = type.GetTypeCache().GetAttribute<DescriptionAttribute>();

            if (attribute != null)
                return attribute.Description;

            return type.Name;
        }

        internal static int GetId<T>(this Either<T, int> either) where T : IPrtgObject
        {
            if (either.IsLeft)
                return either.Left.GetId();

            return either.Right;
        }

        internal static Either<IPrtgObject, int> ToPrtgObject<T>(this Either<T, int> either) where T : IPrtgObject
        {
            if (either.IsLeft)
                return either.Left;

            return either.Right;
        }
    }
}
