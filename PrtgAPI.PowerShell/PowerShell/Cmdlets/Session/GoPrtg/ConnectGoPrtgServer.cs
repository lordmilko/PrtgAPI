using System;
using System.Linq;
using System.Management.Automation;
using System.Security;
using PrtgAPI.PowerShell.GoPrtg;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Connects to a PRTG Network Monitor server using connection details stored in the current PowerShell Profile.</para>
    /// 
    /// <para type="description">GoPrtg allows you to securely store your PRTG Network Monitor server and authentication details inside
    /// your PowerShell profile. Upon opening a new PowerShell Session, by simply executing the GoPrtg command you can be automatically
    /// connected to one of several servers, without needing to enter any credentials.</para>
    /// 
    /// <para type="description">When invoked without arguments, GoPrtg will attempt to connect to the first server in your GoPrtg servers list.
    /// When multiple records are present, a specific server can be connected to by specifying a wildcard pattern that corresponds
    /// to that connection's Server or Alias property. A full list of installed GoPrtg servers can be viewed via the Get-GoPrtgServer cmdlet.</para>
    /// 
    /// <para type="description">Upon connecting to a GoPrtg server, GoPrtg will output a status message to the console indicating
    /// whether the connection succeeded. As GoPrtg depends on the contents of the current user's PowerShell Profile, it is not
    /// recommended to use GoPrtg cmdlets inside scripts. To manage PRTG server connections inside scripts it is recommended
    /// to use the Connect-PrtgServer cmdlet in conjunction with New-Credential / Get-Credential.</para>
    /// 
    /// <example>
    ///     <code>C:\> goprtg</code>
    ///     <para>Connect to the first GoPrtg server installed in the current PowerShell Profile.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> goprtg dev</code>
    ///     <para>Connect to the GoPrtg server with alias "dev"</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> goprtg prtg.exam*</code>
    ///     <para>Connect to the GoPrtg server whose address starts with "prtg.exam".</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Store-Credentials#connection">Online version:</para>
    /// <para type="link">Get-GoPrtgServer</para>
    /// <para type="link">Install-GoPrtgServer</para>
    /// <para type="link">Set-GoPrtgAlias</para>
    /// <para type="link">Uninstall-GoPrtgServer</para>
    /// <para type="link">Update-GoPrtgCredential</para>
    /// <para type="link">Connect-PrtgServer</para>
    /// <para type="link">New-Credential</para>
    /// 
    /// </summary>
    [Cmdlet(VerbsCommunications.Connect, "GoPrtgServer")]
    public sealed class ConnectGoPrtgServer : GoPrtgCmdlet
    {
        /// <summary>
        /// <para type="description">A wildcard expression describing the address or alias of the server to connect to.</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 0)]
        public string Server { get; set; }

        /// <summary>
        /// <para type="description">The type of events to log when -Verbose is specified.</para> 
        /// </summary>
        [Parameter(Mandatory = false)]
        public LogLevel[] LogLevel { get; set; }

        /// <summary>
        /// Performs record-by-record processing for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {            
            if (GoPrtgFunctionInstalled)
            {
                var servers = GetServers();

                if (Server == null)
                {
                    var target = servers.FirstOrDefault();

                    ConnectToGoPrtgServer(target);
                }
                else
                {
                    var wildcard = new WildcardPattern(Server, WildcardOptions.IgnoreCase);

                    var matches = servers.Where(s => wildcard.IsMatch(s.Server) || wildcard.IsMatch(s.Alias)).ToList();

                    if (matches.Count == 1)
                        ConnectToGoPrtgServer(matches.First());
                    else if (matches.Count > 1)
                    {
                        WriteColorOutput("\nAmbiguous server specified. The following servers matched the specified server name or alias", ConsoleColor.Red);

                        var formatted = FormatOutput(matches);

                        WriteObject(formatted, true);
                    }
                    else
                        WriteColorOutput($"\nCould not find a server that matches the name or alias '{Server}'\n", ConsoleColor.Red);
                }
            }
            else
                WriteColorOutput("\nNo GoPrtg servers are installed. Please install a server first using Install-GoPrtgServer\n", ConsoleColor.Red);
        }

        private void ConnectToGoPrtgServer(GoPrtgServer server)
        {
            if (client != null && client.Server == server.Server && client.UserName == server.UserName)
            {
                WriteColorOutput($"\nAlready connected to {server.Server} as {server.UserName}\n", ConsoleColor.Yellow);
            }
            else
            {
                var passhash = InvokeCommand.InvokeScript($"ConvertTo-SecureString {server.PassHash}").First().BaseObject as SecureString;

                var credential = new PSCredential(server.UserName, passhash);

                Connect(server.Server, credential);

                WriteColorOutput($"\nConnected to {server.Server} as {server.UserName}\n", ConsoleColor.Green);
            }

            if (LogLevel != null)
            {
                LogLevel level = PrtgAPI.LogLevel.None;

                foreach (var l in LogLevel)
                    level |= l;

                PrtgSessionState.Client.LogLevel = level;
            }
        }
    }
}
