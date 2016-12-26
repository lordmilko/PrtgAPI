using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PrtgAPI
{
    public class Schedule
    {
        public string Name { get; set; }

        public int Index { get; set; }

        public string RawName { get; set; }

        public Schedule(string raw)
        {
            var regex = new Regex("(.+?)\\|(.+?)\\|");

            var number = regex.Replace(raw, "$1");
            var name = regex.Replace(raw, "$2");

            Index = Convert.ToInt32(number);
            Name = name;

            RawName = raw;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
