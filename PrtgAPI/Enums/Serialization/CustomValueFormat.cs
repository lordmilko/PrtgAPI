namespace PrtgAPI
{
    /// <summary>
    /// Specifies alternate methods of formatting values returned from PRTG Requests.
    /// </summary>
    public enum CustomValueFormat
    {
        /// <summary>
        /// Display as text, using symbols instead of numbers and escaping invalid characters, e.g. five stars (*****) for <see cref="Priority.Five"/> and &amp;amp; instead of &amp;.
        /// </summary>
        Text
    }
}
