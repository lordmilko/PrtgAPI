using System;

namespace PrtgAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    sealed class StandardSplittableString : SplittableStringAttribute
    {
        internal StandardSplittableString() : base(' ', ',')
        {
        }
    }

    /// <summary>
    /// Specifies the characters a string should be split on to form an array.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    class SplittableStringAttribute : Attribute
    {
        /// <summary>
        /// The characters to split on. The first character is the normal character expected by PRTG.
        /// </summary>
        public char[] Characters { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SplittableStringAttribute"/> class.
        /// </summary>
        /// <param name="character">The character to split on.</param>
        /// <param name="alternateChars">Alternate characters that could inadvertently be accepted by PRTG.</param>
        internal SplittableStringAttribute(char character, params char[] alternateChars)
        {
            if (alternateChars != null && alternateChars.Length > 0)
            {
                Characters = new char[alternateChars.Length + 1];
                Characters[0] = character;

                for (var i = 0; i < alternateChars.Length; i++)
                {
                    Characters[i + 1] = alternateChars[i];
                }
            }
            else
            {
                Characters = new[] {character};
            }
        }
    }
}
