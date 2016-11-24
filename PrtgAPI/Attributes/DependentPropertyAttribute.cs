using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    sealed class DependentPropertyAttribute : Attribute
    {
        public string Name { get; private set; }

        public DependentPropertyAttribute(string name)
        {
            Name = name;
        }
    }
}
