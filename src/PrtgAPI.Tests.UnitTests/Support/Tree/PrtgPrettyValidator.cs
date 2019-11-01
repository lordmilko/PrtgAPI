using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tree.Converters.Text;
using PrtgAPI.Tree.Converters.Text.Writers;

namespace PrtgAPI.Tests.UnitTests.Support.Tree
{
    class PrtgPrettyValidator : PrettyWriter
    {
        private string[] expected;

        public PrtgPrettyValidator(string[] expected)
        {
            this.expected = expected;
        }

        public override void Execute(List<PrettyLine> content)
        {
            for (var i = 0; i < content.Count; i++)
            {
                if (i > expected.Length - 1)
                {
                    if (content[i].Text == string.Empty)
                        Assert.Fail("Did not expect \\r\\n");

                    Assert.Fail($"Did not expect '{content[i].Text}'");
                }

                Assert.AreEqual(expected[i], content[i].Text);
            }
        }
    }
}
