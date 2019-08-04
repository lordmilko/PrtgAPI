namespace PrtgAPI.PowerShell
{
    /// <summary>
    /// <para type="description">Wraps an object that may be identified by its name (possibly a wildcard expression) or its <see cref="PrtgObject"/>.</para>
    /// </summary>
    /// <typeparam name="T">The type of object to wrap.</typeparam>
    public class NameOrObject<T> : Either<T, string> where T : PrtgObject
    {
        /// <summary>
        /// The object wrapped by this type. If <see cref="IsObject"/> is false, this value is null.
        /// </summary>
        public T Object => Left;

        /// <summary>
        /// Indicates whether this object contains an <see cref="Object"/>. If false, this object contains a <see cref="Name"/>.
        /// </summary>
        public bool IsObject => IsLeft;

        /// <summary>
        /// The name of this object. If <see cref="IsObject"/> is true, this value is null.
        /// </summary>
        public string Name => Right;

        /// <summary>
        /// Initializes a new instance of the <see cref="NameOrObject{T}"/> class with a <see cref="PrtgObject"/>.
        /// </summary>
        /// <param name="value">The object to wrap.</param>
        public NameOrObject(T value) : base(value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NameOrObject{T}"/> class with an object name or wildcard expression.
        /// </summary>
        /// <param name="value">The name or wildcard expression to wrap.</param>
        public NameOrObject(string value) : base(value)
        {
        }
    }
}
