using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI
{
    /// <summary>
    /// Provides static methods for creating <see cref="FlagEnum{T}"/> objects.<para/>
    /// Note that generic type parameters cannot be inferred when performing an implicit conversion to an interface type.<para/>
    /// As such, for collection types not explicitly covered a known <see cref="FlagEnum"/>.Create method, you must either cast or
    /// convert your argument to a supported parameter type in order for type inference to function properly.
    /// </summary>
    public static class FlagEnum
    {
        /// <summary>
        /// Creates a <see cref="FlagEnum{T}"/> with a specified flag value.
        /// </summary>
        /// <typeparam name="T">The type of enum to encapsulate.</typeparam>
        /// <param name="value">The enum value to encapsulate.</param>
        /// <returns>A <see cref="FlagEnum{T}"/> that encapsulates the specified value.</returns>
        public static FlagEnum<T> Create<T>(T value) where T : struct
        {
            return new FlagEnum<T>(value);
        }

        /// <summary>
        /// Creats a new <see cref="FlagEnum{T}"/> from an enumeration of specified flag values.
        /// </summary>
        /// <typeparam name="T">The type of enum values to encapsulate.</typeparam>
        /// <param name="value">The values to encapsulate.</param>
        /// <returns>A <see cref="FlagEnum{T}"/> that encapsulates the specified values.</returns>
        public static FlagEnum<T> Create<T>(IEnumerable<T> value) where T : struct
        {
            return new FlagEnum<T>(value?.ToArray());
        }

        /// <summary>
        /// Creats a new <see cref="FlagEnum{T}"/> from an array of specified flag values.
        /// </summary>
        /// <typeparam name="T">The type of enum values to encapsulate.</typeparam>
        /// <param name="value">The values to encapsulate.</param>
        /// <returns>A <see cref="FlagEnum{T}"/> that encapsulates the specified values.</returns>
        public static FlagEnum<T> Create<T>(params T[] value) where T : struct
        {
            return new FlagEnum<T>(value);
        }

        /// <summary>
        /// Creats a new <see cref="FlagEnum{T}"/> from a list of specified flag values.
        /// </summary>
        /// <typeparam name="T">The type of enum values to encapsulate.</typeparam>
        /// <param name="value">The values to encapsulate.</param>
        /// <returns>A <see cref="FlagEnum{T}"/> that encapsulates the specified values.</returns>
        public static FlagEnum<T> Create<T>(List<T> value) where T : struct
        {
            return new FlagEnum<T>(value?.ToArray());
        }

        /// <summary>
        /// Creats a new <see cref="FlagEnum{T}"/> from a list of specified flag values.
        /// </summary>
        /// <typeparam name="T">The type of enum values to encapsulate.</typeparam>
        /// <param name="value">The values to encapsulate.</param>
        /// <returns>A <see cref="FlagEnum{T}"/> that encapsulates the specified values.</returns>
        public static FlagEnum<T> Create<T>(ReadOnlyCollection<T> value) where T : struct
        {
            return new FlagEnum<T>(value?.ToArray());
        }

        /// <summary>
        /// Creats a new <see cref="FlagEnum{T}"/> from a list of specified flag values.
        /// </summary>
        /// <typeparam name="T">The type of enum values to encapsulate.</typeparam>
        /// <param name="value">The values to encapsulate.</param>
        /// <returns>A <see cref="FlagEnum{T}"/> that encapsulates the specified values.</returns>
        public static FlagEnum<T> Create<T>(IList<T> value) where T : struct
        {
            return new FlagEnum<T>(value?.ToArray());
        }

        /// <summary>
        /// Creats a new <see cref="FlagEnum{T}"/> from a list of specified flag values.
        /// </summary>
        /// <typeparam name="T">The type of enum values to encapsulate.</typeparam>
        /// <param name="value">The values to encapsulate.</param>
        /// <returns>A <see cref="FlagEnum{T}"/> that encapsulates the specified values.</returns>
        public static FlagEnum<T> Create<T>(IReadOnlyCollection<T> value) where T : struct
        {
            return new FlagEnum<T>(value?.ToArray());
        }

        /// <summary>
        /// Creats a new <see cref="FlagEnum{T}"/> from a list of specified flag values.
        /// </summary>
        /// <typeparam name="T">The type of enum values to encapsulate.</typeparam>
        /// <param name="value">The values to encapsulate.</param>
        /// <returns>A <see cref="FlagEnum{T}"/> that encapsulates the specified values.</returns>
        public static FlagEnum<T> Create<T>(IReadOnlyList<T> value) where T : struct
        {
            return new FlagEnum<T>(value?.ToArray());
        }
    }
}
