using System;
using System.Management.Automation;

namespace PrtgAPI.PowerShell
{
    /// <summary>
    /// <para type="description">Wraps a value that may be a known enum value or a custom expression.</para>
    /// </summary>
    public class EnumOrScriptBlock<TEnum> : Either<ScriptBlock, TEnum> where TEnum : struct
    {
        /// <summary>
        /// Converts an object of one of several types to a <see cref="EnumOrScriptBlock{TEnum}"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <returns>If the value is parsable, a <see cref="EnumOrScriptBlock{TEnum}"/>. Otherwise, an <see cref="ArgumentException"/> will be thrown.</returns>
        public static EnumOrScriptBlock<TEnum> Parse(object value)
        {
            if (value is EnumOrScriptBlock<TEnum>)
                return (EnumOrScriptBlock<TEnum>) value;

            if (value is TEnum)
                return new EnumOrScriptBlock<TEnum>((TEnum) value);

            if (value is ScriptBlock)
                return new EnumOrScriptBlock<TEnum>((ScriptBlock) value);

            TEnum result;

            if (Enum.TryParse(value?.ToString(), true, out result))
                return new EnumOrScriptBlock<TEnum>(result);

            throw new ArgumentException($"'{value}' must be convertable to type '{typeof(TEnum).FullName}' or '{typeof(ScriptBlock).FullName}'.", nameof(value));
        }

        /// <summary>
        /// The script block wrapped by this type. If <see cref="IsScriptBlock"/> is false, this value is null.
        /// </summary>
        public ScriptBlock ScriptBlock => Left;

        /// <summary>
        /// Indicates whether this object contains a <see cref="ScriptBlock"/>. If false, this object contains an enum <see cref="Value"/>.
        /// </summary>
        public bool IsScriptBlock => IsLeft;

        /// <summary>
        /// The name of this object. If <see cref="IsScriptBlock"/> is true, this value is null.
        /// </summary>
        public TEnum Value => Right;

        /// <summary>
        /// Initializes a new instance of the <see cref="NameOrObject{T}"/> class with a <see cref="ScriptBlock"/>.
        /// </summary>
        /// <param name="value">The script block to wrap.</param>
        public EnumOrScriptBlock(ScriptBlock value) : base(value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NameOrObject{T}"/> class with a enum value.
        /// </summary>
        /// <param name="value">The value to wrap.</param>
        public EnumOrScriptBlock(TEnum value) : base(value)
        {
        }
    }
}
