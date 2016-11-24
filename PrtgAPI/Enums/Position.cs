namespace PrtgAPI
{
    /// <summary>
    /// Specifies the direction to move an object when repositioning it within the PRTG User Interface.
    /// </summary>
    public enum Position
    {
        /// <summary>
        /// Move the object up by one position.
        /// </summary>
        Up,

        /// <summary>
        /// Move the object down by one position.
        /// </summary>
        Down,

        /// <summary>
        /// Move the object to the top. All subsequent objects will be shifted down one position.
        /// </summary>
        Top,

        /// <summary>
        /// Move the object to the bottom. All objects previously preceding this object will be shifted up one position.
        /// </summary>
        Bottom
    }
}
