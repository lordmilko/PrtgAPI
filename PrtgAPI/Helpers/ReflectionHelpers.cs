using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace PrtgAPI.Helpers
{
    /// <summary>
    /// Defines helper extension methods used for performing reflection.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ReflectionHelpers
    {
        private static BindingFlags internalFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        /// <summary>
        /// Retrieve the value of an internal property of an object.
        /// </summary>
        /// <param name="obj">The object to retrieve the property from.</param>
        /// <param name="name">The name of the property whose value should be retrieved.</param>
        /// <returns>The value of the retrieved property.</returns>
        public static object GetInternalProperty(this object obj, string name)
        {
            var info = obj.GetInternalPropertyInfo(name);

            if (info == null)
                throw new MissingMemberException(obj.GetType().Name, name);

            return info.GetValue(obj);
        }

        /// <summary>
        /// Retrieve the property info metadata of an internal property.
        /// </summary>
        /// <param name="obj">The object to retrieve the property from.</param>
        /// <param name="name">The name of the property whose info should be retrieved.</param>
        /// <returns>The property info of the specified property. If the property cannot be found or is not internal, this method returns null.</returns>
        public static PropertyInfo GetInternalPropertyInfo(this object obj, string name)
        {
            var prop = obj.GetType().GetProperty(name, internalFlags);

            return prop;
        }

        /// <summary>
        /// Retrieve the value of an internal field of an object.
        /// </summary>
        /// <param name="obj">The object to retrieve the field from.</param>
        /// <param name="name">The name of the field whose value should be retrieved.</param>
        /// <returns>The value of the retrieved field.</returns>
        public static object GetInternalField(this object obj, string name)
        {
            var info = obj.GetInternalFieldInfo(name);

            if (info == null)
                throw new MissingMemberException(obj.GetType().Name, name);

            return info.GetValue(obj);
        }

        /// <summary>
        /// Retrieve the field info metadata of an internal field.
        /// </summary>
        /// <param name="obj">The object to retrieve the field from.</param>
        /// <param name="name">The name of the field whose info should be retrieved.</param>
        /// <returns>The field info of the specified field. If the field cannot be found or is not internal, this method returns null.</returns>
        public static FieldInfo GetInternalFieldInfo(this object obj, string name)
        {
            var field = obj.GetType().GetField(name, internalFlags);

            return field;
        }

        /// <summary>
        /// Retrieve an internal method from an object.
        /// </summary>
        /// <param name="obj">The object to retrieve the method from.</param>
        /// <param name="name">The name of the method to retrieve.</param>
        /// <returns>The method info of the specified method. If the specified method cannot be found or is not internal, this method returns null.<para/>
        /// If more than one method is found with the specified name, this method throws a <see cref="AmbiguousMatchException"/></returns>
        public static MethodInfo GetInternalMethod(this object obj, string name)
        {
            var method = obj.GetType().GetMethod(name, internalFlags);

            return method;
        }

        /// <summary>
        /// Retrieve all properties from a type, excluding those that are not writable or are internal properties used by indexers
        /// </summary>
        /// <param name="type">The type to retrieve properties for.</param>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> GetNormalProperties(this Type type)
        {
            return type.GetProperties().Where(p => !p.GetIndexParameters().Any() && p.CanWrite);
        }

        //https://stackoverflow.com/questions/457676/check-if-a-class-is-derived-from-a-generic-class
        internal static bool IsSubclassOfRawGeneric(this Type type, Type generic)
        {
            while (type != null && type != typeof(object))
            {
                var cur = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
                if (generic == cur)
                {
                    return true;
                }
                type = type.BaseType;
            }
            return false;
        }
    }
}
