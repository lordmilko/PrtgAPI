using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    sealed class RequireValueAttribute : Attribute
    {
        public bool ValueRequired { get; private set; }

        public RequireValueAttribute(bool valueRequired)
        {
            ValueRequired = valueRequired;
        }
    }
}
