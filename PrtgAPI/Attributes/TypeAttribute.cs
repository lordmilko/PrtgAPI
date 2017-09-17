using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Attributes
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    sealed class TypeAttribute : Attribute
    {
        public TypeAttribute(Type @class)
        {
            Class = @class;
        }

        public Type Class { get; private set; }
    }
}
