using System;

namespace PrtgAPI.PowerShell
{
    class ParameterSetDescriptor
    {
        public string Name { get; set; }

        public bool Invoke { get; set; }

        public Enum[] ExcludedParameters { get; set; }

        public ParameterSetDescriptor(string name, bool invoke = false, params Enum[] excludedParameters)
        {
            Name = name;
            Invoke = invoke;
            ExcludedParameters = excludedParameters;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
