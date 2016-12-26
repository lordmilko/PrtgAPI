using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PrtgAPI
{
    public class NotificationActionDescriptor
    {
        public string Name { get; set; }
        public int Id { get; set; }

        public NotificationActionDescriptor(string raw)
        {
            var regex = new Regex("(.+)\\|(.+)");

            var number = regex.Replace(raw, "$1");
            var name = regex.Replace(raw, "$2");

            Id = Convert.ToInt32(number);
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
