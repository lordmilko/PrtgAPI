using System;
using System.Management.Automation;
using System.Reflection;
using PrtgAPI.PowerShell;
using PrtgAPI.PowerShell.Cmdlets;

namespace PrtgAPI.Reflection
{
    static class PSReflectionUtilities
    {
        public static object PSGetInternalStaticField(this object obj, string core, string desktop, PSCmdlet cmdlet)
        {
            var edition = GetEdition(cmdlet);

            if (edition == PSEdition.Core)
                return obj.GetInternalStaticField(core);

            return obj.GetInternalStaticField(desktop);
        }

        public static object PSGetInternalStaticField(this Type type, string core, string desktop, PSCmdlet cmdlet)
        {
            var edition = GetEdition(cmdlet);

            if (edition == PSEdition.Core)
                return type.GetInternalStaticField(core);

            return type.GetInternalStaticField(desktop);
        }

        public static object PSGetInternalField(this object obj, string core, string desktop, PSCmdlet cmdlet)
        {
            var edition = GetEdition(cmdlet);

            if (edition == PSEdition.Core)
                return obj.GetInternalField(core);

            return obj.GetInternalField(desktop);
        }

        public static FieldInfo PSGetInternalFieldInfoFromBase(this Type type, string core, string desktop, PSCmdlet cmdlet)
        {
            var edition = GetEdition(cmdlet);

            if (edition == PSEdition.Core)
                return type.GetInternalFieldInfoFromBase(core);

            return type.GetInternalFieldInfoFromBase(desktop);
        }

        private static PSEdition GetEdition(PSCmdlet cmdlet)
        {
            if (PrtgSessionState.PSEdition != null)
                return PrtgSessionState.PSEdition.Value;

            return ConnectPrtgServer.GetPSEdition(cmdlet);
        }
    }
}
