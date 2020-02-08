using System;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree
{
    [ExcludeFromCodeCoverage]
    class ProbeProxy : GroupOrProbeProxy<Probe>, IProbe
    {
        public ProbeStatus ProbeStatus => Resolved.ProbeStatus;

        public ProbeProxy(Func<Probe> valueResolver) : base(valueResolver)
        {
        }
    }
}
