using System;

namespace PrtgAPI.PowerShell
{
    /// <summary>
    /// Represents a value that may be one of two types.
    /// </summary>
    /// <typeparam name="TLeft">The first type the value may be.</typeparam>
    /// <typeparam name="TRight">The second type the value may be.</typeparam>
    public abstract class Either<TLeft, TRight>
    {
        /// <summary>
        /// The first value that may be specified.
        /// </summary>
        protected TLeft Left { get; }

        /// <summary>
        /// The second value that may be specified.
        /// </summary>
        protected TRight Right { get; }

        /// <summary>
        /// Indicates whether the <see cref="Left"/> value has been specified. If not, <see cref="Right"/> is specified.
        /// </summary>
        protected bool IsLeft { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Either{TLeft, TRight}"/> class with a value of the first possible type.
        /// </summary>
        /// <param name="value">The value to use.</param>
        protected Either(TLeft value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            Left = value;
            IsLeft = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Either{TLeft, TRight}"/> class with a value of the second possible type.
        /// </summary>
        /// <param name="value">The value to use.</param>
        protected Either(TRight value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            Right = value;
            IsLeft = false;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            if (IsLeft)
                return Left.ToString();

            return Right.ToString();
        }
    }
}