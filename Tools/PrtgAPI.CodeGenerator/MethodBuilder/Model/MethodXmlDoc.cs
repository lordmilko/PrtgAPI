using System.Text;

namespace PrtgAPI.CodeGenerator.MethodBuilder.Model
{
    internal class MethodXmlDoc
    {
        public string[] Summary { get; }

        public string[] GenericArgs { get; }

        public string[] Parameters { get; }

        public string[] Exceptions { get; }

        public string[] ReturnDescription { get; }

        public MethodXmlDoc(string[] summary, string[] genericArgs, string[] parameters, string[] exceptions, string[] returnDescription)
        {
            Summary = summary;
            GenericArgs = genericArgs;
            Parameters = parameters;
            Exceptions = exceptions;
            ReturnDescription = returnDescription;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            foreach (var line in Summary)
                builder.AppendLine(line);

            foreach (var arg in GenericArgs)
                builder.AppendLine(arg);

            foreach (var param in Parameters)
                builder.AppendLine(param);

            foreach (var exception in Exceptions)
                builder.AppendLine(exception);

            foreach (var desc in ReturnDescription)
                builder.AppendLine(desc);

            return builder.ToString();
        }

        public void Write(SourceWriter writer)
        {
            foreach (var line in Summary)
                writer.FullLine(line);

            foreach (var arg in GenericArgs)
                writer.FullLine(arg);

            foreach (var param in Parameters)
                writer.FullLine(param);

            foreach (var exception in Exceptions)
                writer.FullLine(exception);

            foreach (var desc in ReturnDescription)
                writer.FullLine(desc);
        }
    }
}
