using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;

namespace PrtgAPI.PowerShell
{
    class RuntimeDefinedParameterEx : RuntimeDefinedParameter
    {
        public RuntimeDefinedParameterEx(string name, Type parameterType, params Attribute[] attributes) : this(name, parameterType, (IEnumerable<Attribute>)attributes)
        {
        }

        public RuntimeDefinedParameterEx(string name, Type parameterType, IEnumerable<Attribute> attributes) : base(
            name, parameterType, new Collection<Attribute>(attributes.ToList()))
        {
        }
    }
}
