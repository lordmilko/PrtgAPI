namespace PrtgAPI.CodeGenerator.Model
{
    internal class Parameter : IParameter
    {
        public string Name { get; }

        public string Type { get; }

        public string Default { get; }

        public string StreamDefault { get; }

        public string Description { get; }

        public string StreamDescription { get; }

        public bool StreamOnly { get; }

        public bool ExcludeStream { get; }

        public bool TokenOnly { get; }

        public string After { get; }

        public Parameter(IParameter parameter)
        {
            Name = parameter.Name;
            Type = parameter.Type;
            Default = parameter.Default;
            StreamDefault = parameter.StreamDefault;
            Description = parameter.Description;
            StreamDescription = parameter.StreamDescription;
            StreamOnly = parameter.StreamOnly;
            ExcludeStream = parameter.ExcludeStream;
            TokenOnly = parameter.TokenOnly;
            After = parameter.After;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}