using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Parameters
{
    public class CustomParameter
    {
        public string Name { get; set; }

        public object Value { get; set; }

        public CustomParameter(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public override string ToString()
        {
            return $"{Name}={Value}";
        }
    }
}
