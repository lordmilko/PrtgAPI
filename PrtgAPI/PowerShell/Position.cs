using System;

namespace PrtgAPI.PowerShell
{
    /// <summary>
    /// <para type="description">Represents an absolute or directional position.</para>
    /// </summary>
    public class Position : Either<int, PrtgAPI.Position>
    {
        /// <summary>
        /// Converts an object of one of several types to a <see cref="Position"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <returns>A <see cref="Position"/> that encapsulates the passed value.</returns>
        public static Position Parse(object value)
        {
            int val;

            if (int.TryParse(value?.ToString(), out val))
                return new Position(val);

            PrtgAPI.Position position;

            if (Enum.TryParse(value?.ToString(), true, out position))
                return new Position(position);

            throw new ArgumentException($"Cannot convert value '{value}' to an absolute or directional position.", nameof(value));
        }

        internal int AbsolutePosition => Left;

        internal PrtgAPI.Position RelativePosition => Right;

        internal bool IsAbsolutePosition => IsLeft;

        private Position(int absolute) : base(absolute)
        {
        }

        private Position(PrtgAPI.Position relative) : base(relative)
        {
        }
    }
}
