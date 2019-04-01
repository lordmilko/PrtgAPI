using System;

namespace PrtgAPI
{
    /// <summary>
    /// Represents a value that may be one of two types.
    /// </summary>
    /// <typeparam name="TLeft">The first type the value may be.</typeparam>
    /// <typeparam name="TRight">The second type the value may be.</typeparam>
    public struct Either<TLeft, TRight>
    {
        /// <summary>
        /// The first value that may be specified.
        /// </summary>
        public TLeft Left { get; }

        /// <summary>
        /// The second value that may be specified.
        /// </summary>
        public TRight Right { get; }

        private bool isLeft;

        /// <summary>
        /// Indicates whether the <see cref="Left"/> value has been specified. If not, <see cref="Right"/> is specified.
        /// </summary>
        public bool IsLeft
        {
            get
            {
                if (state == 0)
                {
                    var types = GetType().GetGenericArguments();

                    throw new InvalidOperationException($"Value of type '{nameof(Either<TLeft, TRight>)}<{types[0].Name}, {types[1].Name}>' was not properly initialized. Value must specify a 'Left' ({types[0].Name}) or 'Right' ({types[1].Name}) value.");
                }

                return isLeft;
            }
        }

        private int state;

        /// <summary>
        /// Initializes a new instance of the <see cref="Either{TLeft, TRight}"/> class with a value of the first possible type.
        /// </summary>
        /// <param name="value">The value to use.</param>
        public Either(TLeft value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value), $"Value of type '{typeof(TLeft).Name}' cannot be null.");

            state = 1;
            Left = value;
            Right = default(TRight);
            isLeft = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Either{TLeft, TRight}"/> class with a value of the second possible type.
        /// </summary>
        /// <param name="value">The value to use.</param>
        public Either(TRight value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value), $"Value of type '{typeof(TRight).Name}' cannot be null.");

            state = 2;
            Right = value;
            Left = default(TLeft);
            isLeft = false;
        }

        /// <summary>
        /// Creates a new <see cref="Either{TLeft, TRight}"/> with a value of the first possible type.
        /// </summary>
        /// <param name="value">The value to use.</param>
        public static implicit operator Either<TLeft, TRight>(TLeft value) => new Either<TLeft, TRight>(value);

        /// <summary>
        /// Creates a new <see cref="Either{TLeft, TRight}"/> with a value of the second possible type.
        /// </summary>
        /// <param name="value">The value to use.</param>
        public static implicit operator Either<TLeft, TRight>(TRight value) => new Either<TLeft, TRight>(value);

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
