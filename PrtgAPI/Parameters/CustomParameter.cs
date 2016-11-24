using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Parameters
{
    class CustomParameter
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public CustomParameter(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
