using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.PowerShell.GoPrtg;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Updates the server alias of the currently active GoPrtg Server.</para>
    /// 
    /// <para type="description">The Set-GoPrtgAlias updates the alias of the currently active GoPrtg Server. GoPrtg
    /// aliases provide a shorthand for connecting to specific GoPrtg servers when multiple servers are installed in
    /// your PowerShell profile. Multiple GoPrtg records may lack aliases as long as they connect to different servers.
    /// Otherwise, each GoPrtg server must have a unique alias. If no alias is specified, the alias of the specified server
    /// will be removed.</para>
    /// 
    /// <example>
    ///     <code>C:\> Set-GoPrtgAlias dev</code>
    ///     <para>Sets the alias of the currently active GoPrtg server to "dev".</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Set-GoPrtgAlias</code>
    ///     <para>Removes the alias of the currently active GoPrtg server.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Store-Credentials#modification">Online version:</para>
    /// <para type="link">Connect-GoPrtgServer</para>
    /// <para type="link">Get-GoPrtgServer</para>
    /// <para type="link">Install-GoPrtgServer</para>
    /// <para type="link">Uninstall-GoPrtgServer</para>
    /// <para type="link">Update-GoPrtgCredential</para>
    ///  
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "GoPrtgAlias")]
    public sealed class SetGoPrtgAlias : GoPrtgCmdlet
    {
        /// <summary>
        /// <para type="description">The alias to assign to the currently active GoPrtg server. If this value is null or empty, the active server's alias will be removed.
        /// If multiple records exist for the active server, a unique alias must be specified.</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 0)]
        public string Alias { get; set; }

        /// <summary>
        /// Provides a one-time, preprocessing functionality for the cmdlet.
        /// </summary>
        protected override void BeginProcessing()
        {
            if (PrtgSessionState.Client == null)
                throw new InvalidOperationException("You are not connected to a PRTG Server. Please connect first using GoPrtg [<server>].");

            AssertProfileExists();
        }

        /// <summary>
        /// Performs record-by-record processing for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            if (Alias == string.Empty)
                Alias = null;
            
            UpdateServerRunner(SetAlias);
        }

        private Action<GoPrtgServer> SetAlias(List<GoPrtgServer> servers, GoPrtgServer activeServer)
        {
            if (servers.Any(s => s.Alias == Alias) && !string.IsNullOrEmpty(Alias))
                throw new InvalidOperationException($"Cannot set alias for server '{client.Server}': a record with alias '{Alias}' already exists. For more information see Get-GoPrtgServer.");

            if (string.IsNullOrEmpty(Alias) && activeServer.Alias != null)
            {
                if (servers.Count(s => s.Server == client.Server) > 1)
                    throw new InvalidOperationException($"Cannot remove alias of server: multiple entries for server '{client.Server}' are stored within GoPrtg. To remove this alias uninstall all other entries for this server. For more information see Get-GoPrtgServer.");
            }

            return server => server.Alias = Alias;
        }
    }
}
