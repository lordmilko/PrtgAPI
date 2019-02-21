using System.Collections.Generic;
using System.Diagnostics;

namespace PrtgAPI.Html
{
    [DebuggerDisplay("Name = {Name}, Options = {Options}")]
    class DropDownList
    {
        public string Name { get; set; }

        public List<Option> Options { get; set; }

        public string Html { get; set; }
    }
}
