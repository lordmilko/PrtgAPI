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
        Text,

        /// <summary>
        /// Display as a raw value without HTML encoding, using numbers instead of symbols and not escaping invalid characters (such as using &amp; instead of &amp;amp;)
        /// </summary>
        NoHtmlEncode
    }
}
