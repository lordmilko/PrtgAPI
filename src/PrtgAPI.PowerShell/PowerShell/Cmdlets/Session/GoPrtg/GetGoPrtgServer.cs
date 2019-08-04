using System;
using System.Linq;
using System.Management.Automation;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieves GoPrtg connection details from the current PowerShell Profile.</para>
    /// 
    /// <para type="description">The Get-GoPrtgServer cmdlet retrieves all GoPrtg connections stored in the current
    /// PowerShell Profile. Each server lists its server address, username used for authentication and alias (if applicable).
    /// The currently active server is indicated by an asterisk. If multiple connections are installed, a wildcard
    /// expression indicating the server addresses or aliases to filter for can be specified to limit results.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-GoPrtgServer</code>
    ///     <para>List all GoPrtg servers installed in the current PowerShell profile.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-GoPrtgServer *dev*</code>
    ///     <para>List all GoPrtg servers whose server address or alias contains the word "dev".</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Store-Credentials#enumeration">Online version:</para>
    /// <para type="link">Connect-GoPrtgServer</para>
    /// <para type="link">Install-GoPrtgServer</para>
    /// <para type="link">Set-GoPrtgAlias</para>
    /// <para type="link">Uninstall-GoPrtgServer</para>
    /// <para type="link">Update-GoPrtgCredential</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "GoPrtgServer")]
    public sealed class GetGoPrtgServer : GoPrtgCmdlet
    {
        /// <summary>
        /// <para type="description">A wildcard expression describing the address or alias of the servers to retrieve.</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 0)]
        public string Server { get; set; }

        /// <summary>
        /// Performs record-by-record processing for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            if (GoPrtgFunctionInstalled)
            {
                var servers = GetServers();

                if (Server != null)
                {
                    var wildcard = new WildcardPattern(Server, WildcardOptions.IgnoreCase);

                    servers = servers.Where(s => wildcard.IsMatch(s.Server) || wildcard.IsMatch(s.Alias)).ToList();
                }

                var response = FormatOutput(servers);

                WriteObject(response, true);
            }
            else
                WriteColorOutput("\nGoPrtg is not installed. Run Install-GoPrtgServer first to install a GoPrtg server.\n", ConsoleColor.Red);
        }
    }
}
