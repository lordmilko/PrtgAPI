using System.Diagnostics;
using System.Text.RegularExpressions;

namespace PrtgAPI.Html
{
    [DebuggerDisplay("{FullHtml,nq}")]
    internal class TableData
    {
        public string Content { get; }

        public string FullHtml { get; }

        public TableData(Match match)
        {
            FullHtml = match.Groups[0].Value;
            Content = match.Groups[1].Value;
        }
    }
}