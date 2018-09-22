namespace PrtgAPI.CodeGenerator.Model
{
    internal class Exception : IException
    {
        public string Type { get; }

        public string Description { get; }

        public Exception(IException ex)
        {
            Type = ex.Type;
            Description = ex.Description;
        }
    }
}