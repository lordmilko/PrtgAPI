namespace PrtgAPI.CodeGenerator
{
    /// <summary>
    /// Represents an abstract model of an XmlDoc &lt;exception/&gt; tag.
    /// </summary>
    internal interface IException
    {
        string Type { get; }

        string Description { get; }
    }
}
