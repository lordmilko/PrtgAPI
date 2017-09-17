using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    sealed class SecondaryPropertyAttribute : Attribute
    {
        public string Name { get; private set; }

        public SecondaryPropertyAttribute(string name)
        {
            this.Name = name;
        }
    }
}
