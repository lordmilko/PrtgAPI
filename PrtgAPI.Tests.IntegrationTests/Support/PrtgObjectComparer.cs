using System.Collections.Generic;

namespace PrtgAPI.Tests.IntegrationTests
{
    class PrtgObjectComparer : IEqualityComparer<PrtgObject>
    {
        public bool Equals(PrtgObject x, PrtgObject y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(PrtgObject obj)
        {
            return obj.GetHashCode();
        }
    }
}
