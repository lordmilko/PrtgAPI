using System.Diagnostics;

namespace PrtgAPI.Html
{
    [DebuggerDisplay("Value = {Value}, Selected = {Selected}, InnerHtml = {InnerHtml}")]
    class Option
    {
        public string Value { get; set; }

        public bool Selected { get; set; }

        public string InnerHtml { get; set; }

        public string Html { get; set; }
    }
}
