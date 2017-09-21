using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Helpers
{
    /// <summary>
    /// Defines helper extension methods used for performing reflection.
    /// </summary>
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
            return obj.GetInternalPropertyInfo(name).GetValue(obj);
        }

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
            return obj.GetInternalFieldInfo(name).GetValue(obj);
        }

        public static FieldInfo GetInternalFieldInfo(this object obj, string name)
        {
            var field = obj.GetType().GetField(name, internalFlags);

            return field;
        }

        public static MethodInfo GetInternalMethod(this object obj, string name)
        {
            var method = obj.GetType().GetMethod(name, internalFlags);

            return method;
        }
    }
}
