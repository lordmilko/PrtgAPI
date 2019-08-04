using System.Diagnostics;

namespace PrtgAPI.Html
{
    [DebuggerDisplay("Name = {Name}, Value = {Value}, Type = {Type}, Checked = {Checked}, Hidden = {Hidden}")]
    class Input
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public InputType Type { get; set; }
        public bool Checked { get; set; }
        public bool Hidden { get; set; }
        public string Html { get; set; }
    }
}
