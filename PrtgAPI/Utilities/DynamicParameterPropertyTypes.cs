using System;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Utilities
{
    [ExcludeFromCodeCoverage]
    class DynamicParameterPropertyTypes
    {
        public string[] String { get; set; }

        public int[] Integer { get; set; }

        public double[] Double { get; set; }

        public StringEnum<ObjectType>[] Type { get; set; }

        public bool[] Boolean { get; set; }

        public ProbeStatus[] ProbeStatus { get; set; }

        public BaseType[] BaseType { get; set; }

        public Access[] Access { get; set; }

        public TimeSpan[] TimeSpan { get; set; }

        public Status[] Status { get; set; }

        public Priority[] Priority { get; set; }

        public DateTime[] DateTime { get; set; }
    }
}
