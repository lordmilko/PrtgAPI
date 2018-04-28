using System.Collections;
using System.Linq;
using System.Management.Automation;

namespace PrtgAPI.Helpers
{
    internal static class PSObjectHelpers
    {
        internal static object CleanPSObject(object obj)
        {
            if (!(obj is string) && obj is IEnumerable)
                return ((IEnumerable)obj).Cast<object>().Select(CleanPSObject).ToArray();

            if (obj is PSObject)
                return ((PSObject)obj).BaseObject;

            return obj;
        }

        internal static object[] CleanPSObject(object[] obj) => obj.Select(CleanPSObject).ToArray();
    }
}
