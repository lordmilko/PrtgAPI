using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PrtgAPI.CodeGenerator.MethodBuilder.Model
{
    internal class MethodHeader
    {
        public string Visibility { get; }

        public string ReturnType { get; }

        public string MethodName { get; }

        public ReadOnlyCollection<HeaderParameter> Parameters { get; }

        public List<string> GenericConstraints { get; }

        public MethodHeader(string visibility, string returnType, string methodName, ReadOnlyCollection<HeaderParameter> parameters, List<string> genericConstraints)
        {
            Visibility = visibility;
            ReturnType = returnType;
            MethodName = methodName;
            Parameters = parameters;
            GenericConstraints = genericConstraints;
        }

        public override string ToString()
        {
            var constraints = string.Join("", GenericConstraints.Select(c => $" where {c}"));

            return $"{Visibility} {ReturnType} {MethodName}({(string.Join(", ", Parameters))}){constraints}";
        }

        public void Write(SourceWriter writer)
        {
            writer.StartLine(ToString());
        }
    }
}
