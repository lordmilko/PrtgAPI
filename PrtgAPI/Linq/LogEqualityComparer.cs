using System.Collections.Generic;
using System.Linq;
using PrtgAPI.Helpers;

namespace PrtgAPI.Linq
{
    class LogEqualityComparer : EqualityComparer<Log>
    {
        public override bool Equals(Log x, Log y)
        {
            return Stringify(x) == Stringify(y);
        }

        public override int GetHashCode(Log obj)
        {
            unchecked
            {
                var result = 0;
                result = (result * 397) ^ Stringify(obj).GetHashCode();

                return result;
            }
        }

        internal static string Stringify(Log log)
        {
            var values = typeof(Log).GetTypeCache().Cache.Properties.Where(p => p.Property.GetSetMethod() != null).Select(p => p.Property.GetValue(log));

            return string.Join("_", values);
        }
    }
}