using System;
using System.Reflection;
using PrtgAPI.PowerShell;

namespace PrtgAPI.Reflection
{
    static class PSReflectionUtilities
    {
        public static object PSGetInternalStaticField(this object obj, string core, string desktop)
        {
            if (PrtgSessionState.PSEdition == PSEdition.Core)
                return obj.GetInternalStaticField(core);

            return obj.GetInternalStaticField(desktop);
        }

        public static object PSGetInternalStaticField(this Type type, string core, string desktop)
        {
            if (PrtgSessionState.PSEdition == PSEdition.Core)
                return type.GetInternalStaticField(core);

            return type.GetInternalStaticField(desktop);
        }

        public static object PSGetInternalField(this object obj, string core, string desktop)
        {
            if (PrtgSessionState.PSEdition == PSEdition.Core)
                return obj.GetInternalField(core);

            return obj.GetInternalField(desktop);
        }

        public static FieldInfo PSGetInternalFieldInfoFromBase(this Type type, string core, string desktop)
        {
            if (PrtgSessionState.PSEdition == PSEdition.Core)
                return type.GetInternalFieldInfoFromBase(core);

            return type.GetInternalFieldInfoFromBase(desktop);
        }
    }
}
