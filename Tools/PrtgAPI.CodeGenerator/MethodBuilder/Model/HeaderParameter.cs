using System.Text;

namespace PrtgAPI.CodeGenerator.MethodBuilder.Model
{
    internal class HeaderParameter
    {
        public string Name { get; }

        public string Type { get; }

        public string DefaultValue { get; }

        public HeaderParameter(string type, string name, string @default)
        {
            Name = name;
            Type = type;
            DefaultValue = @default;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append(Type).Append(" ").Append(Name);

            if (DefaultValue != null)
                builder.Append(" = ").Append(DefaultValue);

            return builder.ToString();
        }
    }
}
