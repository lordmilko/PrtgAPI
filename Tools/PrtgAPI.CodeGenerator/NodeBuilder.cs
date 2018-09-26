using System.Text;

namespace PrtgAPI.CodeGenerator
{
    class NodeBuilder
    {
        private StringBuilder builder = new StringBuilder();

        public string DebugView => builder.ToString();

        public bool Continue { get; set; } = true;

        public int Length
        {
            get { return builder.Length; }
            set { builder.Length = value; }
        }

        public void Append(string value)
        {
            builder.Append(value);
        }

        public void AppendLine()
        {
            builder.AppendLine();
        }

        public void AppendLine(string value)
        {
            builder.AppendLine(value);
        }

        public void Clear()
        {
            builder.Clear();
        }

        public override string ToString()
        {
            return builder.ToString();
        }
    }
}
