using System.Management.Automation;

namespace PrtgAPI.PowerShell
{
    /// <summary>
    /// <para type="description">Wraps a name that may be dynamically generated or manually specified.</para>
    /// </summary>
    public class NameOrScriptBlock : Either<ScriptBlock, string>
    {
        /// <summary>
        /// The script block wrapped by this type. If <see cref="IsScriptBlock"/> is false, this value is null.
        /// </summary>
        public ScriptBlock ScriptBlock => Left;

        /// <summary>
        /// Indicates whether this object contains a <see cref="ScriptBlock"/>. If false, this object contains a <see cref="Name"/>.
        /// </summary>
        public bool IsScriptBlock => IsLeft;

        /// <summary>
        /// The name of this object. If <see cref="IsScriptBlock"/> is true, this value is null.
        /// </summary>
        public string Name => Right;

        /// <summary>
        /// Initializes a new instance of the <see cref="NameOrObject{T}"/> class with a <see cref="ScriptBlock"/>.
        /// </summary>
        /// <param name="value">The script block to wrap.</param>
        public NameOrScriptBlock(ScriptBlock value) : base(value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NameOrObject{T}"/> class with a name to use as a label.
        /// </summary>
        /// <param name="value">The name to wrap.</param>
        public NameOrScriptBlock(string value) : base(value)
        {
        }
    }
}