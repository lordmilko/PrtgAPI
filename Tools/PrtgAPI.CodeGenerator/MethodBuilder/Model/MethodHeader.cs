using System.Collections.ObjectModel;

namespace PrtgAPI.CodeGenerator.MethodBuilder.Model
{
    internal class MethodHeader
    {
        public string Visibility { get; }

        public string ReturnType { get; }

        public string MethodName { get; }

        public ReadOnlyCollection<HeaderParameter> Parameters { get; }

        public MethodHeader(string visibility, string returnType, string methodName, ReadOnlyCollection<HeaderParameter> parameters)
        {
            Visibility = visibility;
            ReturnType = returnType;
            MethodName = methodName;
            Parameters = parameters;
        }

        public override string ToString()
        {
            return $"{Visibility} {ReturnType} {MethodName}({(string.Join(", ", Parameters))})";
        }

        public void Write(SourceWriter writer)
        {
            writer.StartLine(ToString());
        }
    }
}
