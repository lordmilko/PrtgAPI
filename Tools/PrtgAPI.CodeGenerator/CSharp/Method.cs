namespace PrtgAPI.CodeGenerator.CSharp
{
    /// <summary>
    /// Represents a fully constructed method ready to be emitted to a source file.
    /// </summary>
    internal class Method
    {
        public string Name { get; }

        public MethodType Type { get; }

        public string Definition { get; }

        public Method(string name, MethodType type, string definition)
        {
            Name = name;
            Type = type;
            Definition = definition;
        }

        public override string ToString()
        {
            return $"{Name} ({Type})";
        }
    }
}
