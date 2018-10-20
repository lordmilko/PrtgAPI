using System;
using System.IO;

namespace PrtgAPI.Linq.Expressions.Pretty
{
#if DEBUG && DEBUG_SERIALIZATION
    class TextTracker
    {
        public int Line { get; private set; } = 1;

        public int Column { get; private set; } = 1;

        public void WriteText(string str)
        {
            Column += str.Length;
        }

        private void ResetColumn()
        {
            Column = 1;
        }

        public void IncrementLine()
        {
            ResetColumn();
            Line++;
        }
    }

    internal class TextFormatter
    {
        readonly TextWriter writer;

        public string DebugView => writer.ToString();

        bool indentNext;
        int indent;

        public int Indentation => indent;

        TextTracker textTracker = new TextTracker();

        public int Line => textTracker.Line;
        public int Row => textTracker.Column;

        public TextFormatter(TextWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            this.writer = writer;
        }

        void WriteIndent()
        {
            if (!indentNext)
                return;

            for (int i = 0; i < indent; i++)
                writer.Write("    ");
        }

        public void Write(string str)
        {
            WriteIndent();
            textTracker.WriteText(str);
            writer.Write(str);
            indentNext = false;
        }

        public void WriteLine()
        {
            textTracker.IncrementLine();
            writer.WriteLine();
            indentNext = true;
        }

        public void WriteSpace() => Write(" ");

        public void Indent()
        {
            indent++;
        }

        public void Dedent()
        {
            indent--;
        }
    }
#endif
}
