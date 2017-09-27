using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    sealed class MutuallyExclusiveAttribute : Attribute
    {
        public string Name { get; private set; }

        public MutuallyExclusiveAttribute(string name)
        {
            Name = name;
        }
    }
}
