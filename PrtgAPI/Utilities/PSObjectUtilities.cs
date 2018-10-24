using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace PrtgAPI.Utilities
{
    internal static class PSObjectUtilities
    {
        internal static object CleanPSObject(object obj)
        {
            if (obj is PSObject)
                obj = ((PSObject) obj).BaseObject;

            var enumerable = obj as IEnumerable;

            if (enumerable != null && !(enumerable is string))
            {
                var list = new List<object>();
                bool dirty = false;

                foreach (var o in enumerable)
                {
                    var psObject = o as PSObject;

                    if (psObject != null)
                    {
                        list.Add(psObject.BaseObject);
                        dirty = true;
                    }
                    else
                        list.Add(o);
                }

                if (dirty)
                    return list.ToArray();

                return obj;
            }

            return obj;
        }

        internal static object[] CleanPSObject(object[] obj) => obj.Select(CleanPSObject).ToArray();
    }
}
