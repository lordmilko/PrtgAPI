using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;

namespace PrtgAPI.Dynamic
{
    [ExcludeFromCodeCoverage]
    internal abstract class DynamicProxy<T>
    {
        internal static string GetMember => nameof(TryGetMember);
        internal static string SetMember => nameof(TrySetMember);

        internal object lockObject = new object();

        /// <summary>
        /// Retrieves the value of a property from a dynamic object.
        /// </summary>
        /// <param name="instance">The dynamic object to retrieve the property from.</param>
        /// <param name="binder">The binder that specifies the property to access.</param>
        /// <param name="value">Returns the value of the property set by this method.</param>
        /// <returns>True if the member was successfully retrieved. Otherwise, false.</returns>
        public virtual bool TryGetMember(T instance, GetMemberBinder binder, out object value)
        {
            lock (lockObject)
            {
                value = null;

                return false;
            }
        }

        /// <summary>
        /// Sets the value of a property on a dynamic object.
        /// </summary>
        /// <param name="instance">The dynamic object to set a property on.</param>
        /// <param name="binder">The binder that specifies the property to modify.</param>
        /// <param name="value">The value to set the property to.</param>
        /// <returns>True if the member was successfully set. Otherwise, false.</returns>
        public virtual bool TrySetMember(T instance, SetMemberBinder binder, object value)
        {
            return false;
        }

        /// <summary>
        /// Retrieves a list of all dynamic properties defined on a dynamic object.
        /// </summary>
        /// <param name="instance">The dynamic object to list the dynamic properties of.</param>
        /// <returns>A list of all dynamic properties defined on the specified object.</returns>
        public virtual IEnumerable<string> GetDynamicMemberNames(T instance)
        {
            return Enumerable.Empty<string>();
        }
    }
}
