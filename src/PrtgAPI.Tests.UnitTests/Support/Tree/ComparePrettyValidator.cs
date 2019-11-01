using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tree.Converters.Text;
using PrtgAPI.Tree.Converters.Text.Writers;

namespace PrtgAPI.Tests.UnitTests.Support.Tree
{
    class ComparePrettyValidator : PrettyColorWriter
    {
        private StringBuilder builder = new StringBuilder();
        private string[] expected;

        protected override void Write(string text)
        {
            builder.Append(text);
        }

        protected override void WriteLine(ConsoleColor? color, string text)
        {
            if (color == null)
                builder.Append(text);
            else
                builder.Append($"<{color}>{text}</{color}>");

            builder.Append("\r\n");
        }

        public ComparePrettyValidator(string[] expected)
        {
            this.expected = expected;
        }

        public override void Execute(List<PrettyLine> lines)
        {
            base.Execute(lines);

            var split = builder.ToString().Split(new[] { "\r\n" }, StringSplitOptions.None);
            split = split.Take(split.Length - 1).ToArray();

            for (var i = 0; i < split.Length; i++)
            {
                if (i > expected.Length - 1)
                {
                    if (split[i] == string.Empty)
                        Assert.Fail("Did not expect \\r\\n");

                    Assert.Fail($"Did not expect '{split[i]}'");
                }

                Assert.AreEqual(expected[i], split[i]);
            }
        }
    }
}
