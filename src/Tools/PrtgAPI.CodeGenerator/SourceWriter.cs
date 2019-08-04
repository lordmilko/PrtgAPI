using System.Text;

namespace PrtgAPI.CodeGenerator
{
    public class SourceWriter
    {
        private StringBuilder builder = new StringBuilder();

        public void StartRegion(string name, int level)
        {
            Indent(level);

            builder.AppendLine($"#region {name}");
        }

        public void EndRegion(int level)
        {
            Indent(level);

            builder.AppendLine("#endregion");
        }

        private void Indent(int? count = null)
        {
            if (count == null)
                count = 2;

            for (var i = 0; i < count.Value; i++)
                builder.Append("    ");
        }

        public string DebugView => ToString();

        public override string ToString()
        {
            return builder.ToString();
        }

        public void FullLine(string value)
        {
            Indent();
            builder.AppendLine(value);
        }

        public void StartLine(string value)
        {
            Indent();
            builder.Append(value);
        }

        public void Write(string value)
        {
            builder.Append(value);
        }

        public void WriteLine(string value)
        {
            builder.AppendLine(value);
        }
    }
}