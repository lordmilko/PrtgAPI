using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.PowerShell.GoPrtg;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Uninstalls one or more GoPrtg servers from the current PowerShell Profile.</para>
    /// 
    /// <para type="description">The Uninstall-GoPrtgServer cmdlet uninstalls one or more GoPrtg servers from the current
    /// PowerShell Profile. If a single GoPrtg server exists in your PowerShell Profile, invoking Uninstall-GoPrtgServer with
    /// no arguments will automatically install your single server. Otherwise, a -<see cref="Server"/> wildcard expression must
    /// be specified matching one or more servers or aliases to remove. Alternatively, you may specify the -<see cref="Force"/>
    /// parameter to automatically remove all GoPrtg servers.</para>
    /// 
    /// <example>
    ///     <code>C:\> Uninstall-GoPrtgServer *dev*</code>
    ///     <para>Uninstalls all GoPrtg servers whose name or alias contains "dev".</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Uninstall-GoPrtgServer -Force</code>
    ///     <para>Uninstalls all GoPrtg servers.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Store-Credentials#uninstallation">Online version:</para>
    /// <para type="link">Connect-GoPrtgServer</para>
    /// <para type="link">Get-GoPrtgServer</para>
    /// <para type="link">Install-GoPrtgServer</para>
    /// <para type="link">Set-GoPrtgAlias</para>
    /// <para type="link">Update-GoPrtgCredential</para>
    /// 
    /// </summary>
    [Cmdlet(VerbsLifecycle.Uninstall, "GoPrtgServer")]
    public sealed class UninstallGoPrtgServer : GoPrtgCmdlet
    {
        /// <summary>
        /// <para type="description">A wildcard describing the servers to remove. If only one GoPrtg server exists,
        /// this parameter can be omitted.</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 0, ParameterSetName = ParameterSet.Default)]
        public string Server { get; set; }

        /// <summary>
        /// <para type="description">Specifies that all GoPrtg servers should be removed.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Force)]
        public SwitchParameter Force { get; set; }

        /// <summary>
        /// Provides a one-time, preprocessing functionality for the cmdlet.
        /// </summary>
        protected override void BeginProcessing()
        {
            AssertProfileExists();
        }

        /// <summary>
        /// Performs record-by-record processing for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            ValidateGoPrtgBlock();

            if (GoPrtgFunctionInstalled)
            {
                var newContent = GetNewContent();

                if (newContent.Count > 0)
                    newContent = AddGoPrtgFunctionHeaderAndFooter(newContent);

                UpdateGoPrtgFunctionBody(newContent);

                if (newContent != null && newContent.Count > 0)
                {
                    LoadFunction(string.Join(Environment.NewLine, newContent));
                }
                else
                    RemoveFunction();
            }
            else
                throw new InvalidOperationException("No GoPrtg servers are installed. To install a GoPrtg server, run Install-GoPrtgServer.");
        }

        private List<GoPrtgServer> GetMatches()
        {
            var servers = GetServers();

            var server = SetServerWildcardIfMissing(servers);

            var wildcard = new WildcardPattern(server, WildcardOptions.IgnoreCase);

            servers = servers.Where(s => wildcard.IsMatch(s.Server) || wildcard.IsMatch(s.Alias)).ToList();

            if (servers.Count == 0)
                throw new InvalidOperationException($"'{server}' is not a valid server name or alias. To view all saved servers, run Get-GoPrtgServer.");

            return servers;
        }

        private List<string> GetNewContent()
        {
            var matches = GetMatches();

            var newContent = ForeachServer(line =>
            {
                //Include the line if none of the servers were a match for it
                var include = matches.Select(FormatWildcardTarget)
                                     .Select(str => new WildcardPattern(str, WildcardOptions.IgnoreCase))
                                     .All(wildcard => !wildcard.IsMatch(line));

                return include ? line : null;
            });

            if (newContent.Count > 0)
            {
                AdjustFilterRecordDelimiters(newContent);
            }

            return newContent;
        }

        private void AdjustFilterRecordDelimiters(List<string> newContent)
        {
            for (var i = 0; i < newContent.Count; i++)
            {
                if (i < newContent.Count - 1)
                {
                    //All records prior to the last one should end in a comma
                    if (newContent[i].EndsWith(",") == false)
                        newContent[i] += ",";
                }
                else
                {
                    //The last record should not end in a comma
                    if (newContent[i].EndsWith(","))
                        newContent[i] = newContent[i].Substring(0, newContent[i].Length - 1);
                }
            }
        }

        private string SetServerWildcardIfMissing(List<GoPrtgServer> servers)
        {
            var server = Server;

            if (Force)
                server = "*";
            else
            {
                if (string.IsNullOrEmpty(Server))
                {
                    if (servers.Count > 1)
                        throw new InvalidOperationException("Cannot remove servers; server name or alias must be specified when multiple entries exist. To remove all servers, specify -Force.");

                    server = "*";
                }
            }

            return server;
        }
    }
}
