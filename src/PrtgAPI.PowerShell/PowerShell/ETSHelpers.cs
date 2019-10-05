using System;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.Tree;

namespace PrtgAPI.PowerShell
{
    /// <summary>
    /// Provides helper methods that can be invoked by the PowerShell Extended Type System.
    /// </summary>
    public static class ETSHelpers
    {
        /// <summary>
        /// Wraps a <see cref="ScriptBlock"/> in a function that passes a <see cref="PrtgNode"/> as $_.
        /// </summary>
        /// <param name="scriptBlock">The script block to wrap.</param>
        /// <returns>A function that invokes the script block with a <see cref="PrtgNode"/> as $_.</returns>
        public static Func<PrtgNode, bool> PrtgNodePredicate(ScriptBlock scriptBlock)
        {
            Func<PrtgNode, bool> predicate = node =>
            {
                if (scriptBlock == null)
                    return true;

                var result = scriptBlock.InvokeWithDollarUnder(node);

                var obj = result?.FirstOrDefault()?.BaseObject;

                if (obj is bool)
                    return (bool) obj;

                return false;
            };

            return predicate;
        }
    }
}
