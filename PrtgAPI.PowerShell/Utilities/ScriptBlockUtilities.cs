using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;

namespace PrtgAPI.PowerShell
{
    static class ScriptBlockUtilities
    {
        internal static Collection<PSObject> InvokeWithDollarUnder(this ScriptBlock scriptBlock, object dollarUnder, params PSVariable[] variables)
        {
            if (dollarUnder == null)
                throw new ArgumentNullException(nameof(dollarUnder));

            var list = new List<PSVariable>();

            list.Add(new PSVariable("_", dollarUnder));
            list.AddRange(variables);

            return InvokeWithVariables(scriptBlock, list.ToArray());
        }

        internal static Collection<PSObject> InvokeWithVariables(this ScriptBlock scriptBlock, params PSVariable[] variables)
        {
            return scriptBlock.InvokeWithContext(null, variables.ToList());
        }
    }
}
