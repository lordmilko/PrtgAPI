using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Html
{
    class Input
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public InputType Type { get; set; }
        public bool Checked { get; set; }
        public bool Hidden { get; set; }
    }
}
