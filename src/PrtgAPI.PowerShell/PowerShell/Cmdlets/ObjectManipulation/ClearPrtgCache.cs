using System;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Clears cached data used by a PRTG Network Monitor server.</para>
    /// 
    /// <para type="description">The Clear-PrtgCache cmdlet clears Geo Map, Active Directory authentication
    /// and Graph Data details from a PRTG server. Geo Map and Active Directory authentication caches are grouped
    /// together under the <see cref="SystemCacheType.General"/> cache type. Graph Data is cleared separately,
    /// under the <see cref="SystemCacheType.GraphData"/> cache type.</para>
    /// 
    /// <para type="description">When Graph Data is cleared, the PRTG Core Service will automatically restart.
    /// To prevent this happening accidentally, Clear-PrtgCache will prompt you to confirm you are sure you wish to proceed.
    /// This can be overridden by specifying the -<see cref="Force"/> parameter.</para>
    /// 
    /// <example>
    ///     <code>C:\> Clear-PrtgCache General</code>
    ///     <para>Clear Geo Map and Active Directory authentication caches.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Clear-PrtgCache GraphData -Force</code>
    ///     <para>Clear PRTG's graph data cache without displaying a confirmation prompt that this will restart the PRTG Core Service.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Administrative-Tools#clear-system-caches-1">Online version:</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Clear, "PrtgCache", SupportsShouldProcess = true)]
    public class ClearPrtgCache : PrtgCmdlet
    {
        /// <summary>
        /// <para type="description">The type of cache to clear.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public SystemCacheType Type { get; set; }

        /// <summary>
        /// <para type="description">Forces PRTG to clear caches that may cause a reboot of the PRTG Server without displaying a confirmation prompt.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Force { get; set; }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (ShouldProcess($"Clear PRTG {GetCacheDescription(Type)} Cache"))
            {
                if (Type == SystemCacheType.GraphData)
                {
                    bool yes = false;
                    bool no = false;

                    if (Force || ShouldContinue("Clearing the graph cache will restart the PRTG Core Service. All users will be disconnected while restart is in progress. Are you sure you wish to continue?", "WARNING!", true, ref yes, ref no))
                    {
                        client.ClearSystemCache(Type);
                    }
                }
                else
                {
                    client.ClearSystemCache(Type);
                }
            }

            client.ClearSystemCache(Type);
        }

        [ExcludeFromCodeCoverage]
        private string GetCacheDescription(SystemCacheType cacheType)
        {
            if (cacheType == SystemCacheType.General)
                return "Map & Authentication";
            if (cacheType == SystemCacheType.GraphData)
                return "Graph Data";

            throw new NotImplementedException($"Don't know how to handle cache type '{cacheType}'.");
        }
    }
}
