namespace PrtgAPI.CodeGenerator.Model
{
    internal class GenericArg
    {
        public string Name { get; }

        public string Description { get; }

        public string Constraint { get; }

        public GenericArg(IGenericArg arg)
        {
            Name = arg.Name;
            Description = arg.Description;
            Constraint = arg.Constraint;
        }
    }
}
