namespace PrtgAPI.CodeGenerator
{
    /// <summary>
    /// Represents an abstract model of an XmlDoc &lt;typeparam/&gt; tag.
    /// </summary>
    internal interface IGenericArg
    {
        string Name { get; }

        string Description { get; }

        string Constraint { get; }
    }
}
