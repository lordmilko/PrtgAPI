namespace PrtgAPI.CodeGenerator
{
    /// <summary>
    /// Represents an abstract model of a method parameter.
    /// </summary>
    internal interface IParameter
    {
        string Name { get; }

        string Type { get; }

        string Default { get; }

        string StreamDefault { get; }

        string Description { get; }

        string StreamDescription { get; }

        bool StreamOnly { get; }

        bool ExcludeStream { get; }

        bool TokenOnly { get; }

        string After { get; }
    }
}
