using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Helpers
{
    public static class ReflectionHelpers
    {
        public static object GetInternalProperty(this object obj, string name)
        {
            var internalFlags = BindingFlags.Instance | BindingFlags.NonPublic;

            var prop = obj.GetType().GetProperty(name, internalFlags).GetValue(obj);

            return prop;
        }

        public static object GetInternalField(this object obj, string name)
        {
            var internalFlags = BindingFlags.Instance | BindingFlags.NonPublic;

            var field = obj.GetType().GetField(name, internalFlags).GetValue(obj);

            return field;
        }
    }
}
