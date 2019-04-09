using System;
using System.Linq;

namespace PrtgAPI.CodeGenerator.MethodBuilder.Model
{
    internal class MethodBody
    {
        public bool IsExpression { get; }

        public bool SingleLineExpression { get; }

        public string[] Body { get; }

        public MethodBody(bool isExpression, bool singleLineExpression, string[] body)
        {
            IsExpression = isExpression;
            SingleLineExpression = singleLineExpression;
            Body = body;
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, Body);
        }

        public void Write(SourceWriter writer)
        {
            if (IsExpression)
            {
                if (Body.Length != 1)
                    throw new InvalidOperationException($"Method body {ToString()} should be expression but is not exactly 1 line");

                if (SingleLineExpression)
                    writer.WriteLine($" => {Body.Single()};");
                else
                {
                    writer.WriteLine(" =>");
                    writer.FullLine($"    {Body.Single()};");
                }
            }
            else
            {
                writer.WriteLine("");

                writer.FullLine("{");

                for (var i = 0; i < Body.Length; i++)
                {
                    if (Body[i] == string.Empty)
                        writer.WriteLine("");
                    else
                        writer.FullLine($"    {Body[i]}");
                }

                writer.FullLine("}");
            }
        }
    }
}
