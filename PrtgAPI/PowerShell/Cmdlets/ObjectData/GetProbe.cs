using System.Collections.Generic;
using System.Management.Automation;
using System.Threading.Tasks;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// Retrieve probes from a PRTG Server.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "Probe")]
    public class GetProbe : PrtgTableCmdlet<Probe>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetProbe"/> class.
        /// </summary>
        public GetProbe() : base(Content.Objects, null)
        {
        }

        /// <summary>
        /// Retrieves all probes from a PRTG Server.
        /// </summary>
        /// <returns>A list of all probes.</returns>
        protected override IEnumerable<Probe> GetRecords()
        {
            return client.GetProbes();
        }

        /// <summary>
        /// Retrieves a list of probes from a PRTG Server based on a specified filter.
        /// </summary>
        /// <param name="filter">A list of filters to use to limit search results.</param>
        /// <returns>A list of probes that match the specified search criteria.</returns>
        protected override IEnumerable<Probe> GetRecords(params SearchFilter[] filter)
        {
            return client.GetProbes(filter);
        }
    }
}
