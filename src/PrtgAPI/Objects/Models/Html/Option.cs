using System.Diagnostics;

namespace PrtgAPI.Html
{
    [DebuggerDisplay("Value = {Value}, Selected = {Selected}")]
    class Option
    {
        public string Value { get; set; }

        public bool Selected { get; set; }

        public string Html { get; set; }
    }
}
