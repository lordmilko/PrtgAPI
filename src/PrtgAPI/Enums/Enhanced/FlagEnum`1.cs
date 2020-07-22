using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PrtgAPI.Reflection;
using PrtgAPI.Utilities;

namespace PrtgAPI
{
    /// <summary>
    /// Provides a wrapper around an enum decorated with a <see cref="FlagsAttribute"/>, allowing easy analysis
    /// of the flags contained in a value.
    /// </summary>
    /// <typeparam name="T">The type of enum to encapsulate.</typeparam>
    public struct FlagEnum<T> : IEquatable<T> where T : struct
    {
        private T value;
        private T[] values;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="FlagEnum{T}"/> class with a specified flag value.
        /// </summary>
        /// <param name="value">The enum value to encapsulate.</param>
        public FlagEnum(T value)
        {
            if (!(typeof(T).IsEnum))
                throw new ArgumentException("Value must be an enum.", nameof(value));

            var underlying = ((Enum) (object) value).GetUnderlyingFlags().Cast<T>().ToArray();

            if (underlying.Length > 0)
            {
                //We can't just assign the value directly, as if a negative enum was specified,
                //the value is now every value _but_ that
                this.value = MergeFlags(underlying);

                values = underlying;
            }
            else
            {
                this.value = value;

                values = new[] {value};
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlagEnum{T}"/> with the individual enum values to to encapsulate.<para/>
        /// If any enum values contain underlying flag values, these values will automatically be expanded.
        /// </summary>
        /// <param name="values">The individual enum flags to encapsulate.</param>
        public FlagEnum(params T[] values)
        {
            if (!(typeof(T).IsEnum))
                throw new ArgumentException("Value must be an enum.", nameof(values));

            if (values == null || values.Length == 0)
                values = new[] { default(T) };

            var defined = values.Where(v => ((Enum) (object) v).GetEnumFieldCache() != null).ToArray();
            var undefined = values.Except(defined).ToArray();

            if (undefined.Length > 0)
            {
                var mergeUndefined = undefined.Select(u => Convert.ToInt32(u))
                    .Aggregate((current, next) => current & next);

                if (defined.Length > 0)
                {
                    var mergedDefined = Convert.ToInt32(MergeFlags(defined));

                    var updatedValue = (T) (object) (mergedDefined & mergeUndefined);

                    values = new[] {updatedValue};
                }
                else
                    values = new[] { (T) (object) mergeUndefined };
            }

            var definedFlags = values.Select(v =>
                {
                    var u = ((Enum) (object) v).GetUnderlyingFlags().Cast<T>().ToArray();

                    if (u.Length == 0)
                        return new[] { v };
                    else
                        return u;
                })
                .SelectMany(v => v).Cast<T>().ToArray();

            var definedFlagsAggregated = MergeFlags(definedFlags);

            this.value = definedFlagsAggregated;
            this.values = definedFlags;
        }

        private static T MergeFlags(T[] flags)
        {
            return (T) (object) flags.Select(v => Convert.ToInt32(v))
                .Aggregate((current, next) => current | next);
        }

        #endregion

        /// <summary>
        /// Indicates whether the flags of this enum contain a specified value.
        /// </summary>
        /// <param name="value">The value to search for.</param>
        /// <returns>True if this enum's flags contain the specific value. Otherwise, false.</returns>
        public bool Contains(T value)
        {
            return GetValuesSafe().Contains(value);
        }

        /// <summary>
        /// Creates a new <see cref="FlagEnum{T}"/>from a specified enum value.
        /// </summary>
        /// <param name="value">The value to encapsulate.</param>
        public static implicit operator FlagEnum<T>(T value)
        {
            return new FlagEnum<T>(value);
        }

        /// <summary>
        /// Creates a new <see cref="FlagEnum{T}"/>from one or more specified enum values.
        /// </summary>
        /// <param name="values">The values to encapsulate.</param>
        public static implicit operator FlagEnum<T>(T[] values)
        {
            return new FlagEnum<T>(values); //todo: needs a unit test
        }

        /// <summary>
        /// Unwraps the underlying enum value of a specified <see cref="FlagEnum{T}"/>.
        /// </summary>
        /// <param name="value">The flag emum to unwrap.</param>
        public static implicit operator T(FlagEnum<T> value)
        {
            return value.value;
        }

        /// <summary>
        /// Performs a bitwise logical AND on the enum values of two <see cref="FlagEnum{T}"/> objects.
        /// </summary>
        /// <param name="left">The first operand.</param>
        /// <param name="right">The second operand.</param>
        /// <returns>The result of performing a bitwise logical AND on the enum values of both operands.</returns>
        public static FlagEnum<T> operator &(FlagEnum<T> left, FlagEnum<T> right)
        {
            return (T) (object) ((int) (object) left.value & (int) (object) right.value);
        }

        /// <summary>
        /// Performs a bitwise logical OR on the enum values of two <see cref="FlagEnum{T}"/> objects.
        /// </summary>
        /// <param name="left">The first operand.</param>
        /// <param name="right">The second operand.</param>
        /// <returns>The result of performing a bitwise logical OR on the enum values of both operands.</returns>
        public static FlagEnum<T> operator |(FlagEnum<T> left, FlagEnum<T> right)
        {
            return (T) (object) ((int) (object) left.value | (int) (object) right.value);
        }

        /// <summary>
        /// Performs a bitwise complement on the enum value contained in a single <see cref="FlagEnum{T}"/>
        /// and returns a new <see cref="FlagEnum{T}"/> containing every every enum that was not in the original value.
        /// </summary>
        /// <param name="value">The operand to process.</param>
        /// <returns>The result of performing a bitwise complement of the specified operand.</returns>
        public static FlagEnum<T> operator ~(FlagEnum<T> value)
        {
            var integer = ~Convert.ToInt32(value.value);

            return (T) (object) integer;
        }

        /// <summary>
        /// Retrieves the merged enum contained in this object.
        /// </summary>
        public T Value => value;

        /// <summary>
        /// Retrieves all of the flags contained in the enum.
        /// </summary>
        /// <returns>The flags contained in the enum.</returns>
        public IReadOnlyList<T> GetValues()
        {
            return new ReadOnlyCollection<T>(GetValuesSafe());
        }

        private T[] GetValuesSafe()
        {
            //If struct was initialized using default constructor, values will be null.
            //Initialise values to a collection containing the default enum value.
            if (values == null)
                values = new[] { value };

            return values;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return string.Join(", ", GetValuesSafe());
        }

        /// <summary>
        /// Returns a boolean indicating whether the specified enum value is equal
        /// to the enum value contained in this object.
        /// </summary>
        /// <param name="other">The value to compare with the enum contained in this object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(T other)
        {
            return value.Equals(other);
        }

        /// <summary>
        /// Returns a boolean indicating whether the specified object <paramref name="obj"/>
        /// is equal to this object. The specified object is equal its enum value is the same
        /// as the value contained in this object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (obj is FlagEnum<T>)
                return Equals(((FlagEnum<T>) obj).value);

            return value.Equals(obj);
        }

        /// <summary>
        /// Returns a hash code for this object. If two objects represent the same
        /// enum value, they will have the same hash code.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
    }
}
