using System.Collections.Generic;
using System.Management.Automation;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// Retrieve probes from a PRTG Server.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "Probe")]
    public class GetProbe : PrtgTableCmdlet<Probe, ProbeParameters>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetProbe"/> class.
        /// </summary>
        public GetProbe() : base(Content.Objects, null)
        {
        }

        /// <summary>
        /// Creates a new parameter object to be used for retrieving probes from a PRTG Server.
        /// </summary>
        /// <returns></returns>
        protected override ProbeParameters CreateParameters() => new ProbeParameters();
    }
}
