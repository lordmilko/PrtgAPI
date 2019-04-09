using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using PrtgAPI.PowerShell.GoPrtg;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Installs the active PRTG Network Monitor server as a GoPrtg server in the current PowerShell Profile.</para>
    /// 
    /// <para type="description">The Install-GoPrtgServer cmdlet installs the active PRTG Network Monitor server as a GoPrtg server in the current
    /// PowerShell Profile. GoPrtg servers provide a means of securely connecting to PRTG servers without having to re-enter
    /// your PRTG server and authentication details every time you open a new PowerShell Session or wish to switch to another server.</para>
    /// 
    /// <para type="description">When installing a new GoPrtg server, you may optionally specify an -<see cref="Alias"/> for your connection
    /// to use a shorthand when using the GoPrtg command to toggle between servers. Multiple records may lack an alias as long
    /// as they are for different servers. Otherwise, each record stored by Install-GoPrtgServer MUST have a unique
    /// alias. The alias for the currently active GoPrtg server can be updated via the Update-GoPrtgCredential cmdlet.</para>
    /// 
    /// <para type="description">When storing your connection details, Install-GoPrtgServer encrypts your PassHash as a SecureString,
    /// which is then converted to an Encrypted String and stored alongside your server, username and alias (if applicable). When
    /// stored as an Encrypted String, your PassHash can only be decrypted under your user account on the computer on which
    /// it was created.</para>
    /// 
    /// <para type="description">When GoPrtg servers have been installed, special code will be inserted into your PowerShell Profile,
    /// demarcated by a special header and footer indicating the bounds of GoPrtg's configuration data. Great care should be taken
    /// when manipulating your PowerShell Profile. If any key markings (including the GoPrtg headers and footers themselves) are removed
    /// from the PowerShell Profile, GoPrtg may be unable to continue parsing its configuration without manual intervention. If worst comes
    /// to worst, you can repair your corrupted GoPrtg configuration by simply removing the entire GoPrtg section (including headers and footers)
    /// and reinstalling your GoPrtg servers.</para>
    /// 
    /// <example>
    ///     <code>C:\> Install-GoPrtg</code>
    ///     <para>Installs the currently active server without an alias.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Install-GoPrtg dev</code>
    ///     <para>Installs the currently active server with the alias "dev"</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Store-Credentials#installation">Online version:</para>
    /// <para type="link">Connect-GoPrtgServer</para> 
    /// <para type="link">Get-GoPrtgServer</para> 
    /// <para type="link">Set-GoPrtgAlias</para> 
    /// <para type="link">Uninstall-GoPrtgServer</para> 
    /// <para type="link">Update-GoPrtgCredential</para> 
    /// <para type="link">Connect-PrtgServer</para> 
    /// 
    /// </summary>
    [Cmdlet(VerbsLifecycle.Install, "GoPrtgServer")]
    public sealed class InstallGoPrtgServer : GoPrtgCmdlet
    {
        /// <summary>
        /// <para type="description">An optional alias to associate with the server. An alias must be specified if an existing record exists for the currently active server.</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 0)]
        public string Alias { get; set; }

        /// <summary>
        /// Provides a one-time, preprocessing functionality for the cmdlet.
        /// </summary>
        protected override void BeginProcessing()
        {
            if (PrtgSessionState.Client == null)
                throw new InvalidOperationException("You are not connected to a PRTG Server. Please connect first using Connect-PrtgServer.");
        }

        /// <summary>
        /// Performs record-by-record processing for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            if (Alias == string.Empty)
                Alias = null;

            var isNew = false;

            var info = new FileInfo(profile);

            if (profile != null && !Directory.Exists(info.DirectoryName))
            {
                Directory.CreateDirectory(info.DirectoryName);
                isNew = true;
            }

            var newContents = GetNewContents();
            var profileContent = GetProfileContents(newContents);

            if (isNew)
            {
                File.AppendAllText(profile, profileContent);
            }
            else
            {
                File.WriteAllText(profile, profileContent);
            }

            LoadFunction(string.Join(Environment.NewLine, newContents.Func));
        }

        private GoPrtgProfileContents GetNewContents()
        {
            ValidateGoPrtgBlock();

            if (GoPrtgFunctionInstalled)
            {
                var servers = GetServers();

                CheckServerAgainstExistingRecords(servers);

                var newContent = GetNewContentsInternal(servers);

                var obj = new GoPrtgProfileContents(GoPrtgProfile.AboveHeader.Value, newContent, GoPrtgProfile.BelowFooter.Value);

                return obj;
            }
            else
            {
                var newContent = GetNewContentsInternal(null);

                var obj = new GoPrtgProfileContents(GoPrtgProfile.AboveHeader.Value, newContent, null);

                return obj;
            }
        }

        private string GetProfileContents(GoPrtgProfileContents profileContents)
        {
            var builder = new StringBuilder();

            if (profileContents.Pre != null && profileContents.Pre.Count > 0)
                builder.Append(string.Join(Environment.NewLine, profileContents.Pre)).Append(Environment.NewLine);

            builder.Append(GoPrtgProfile.GoPrtgHeader)
                   .Append(Environment.NewLine)
                   .Append(Environment.NewLine)
                   .Append(string.Join(Environment.NewLine, profileContents.Func))
                   .Append(Environment.NewLine)
                   .Append(Environment.NewLine)
                   .Append(GoPrtgProfile.GoPrtgFooter);

            if (profileContents.Post != null && profileContents.Post.Count > 0)
                builder.Append(Environment.NewLine).Append(string.Join(Environment.NewLine, profileContents.Post));

            //Final trailing newline. Would be automatically entered by Add-Content / Set-Content
            builder.Append(Environment.NewLine);

            return builder.ToString();
        }

        private List<string> GetNewContentsInternal(List<GoPrtgServer> servers)
        {
            if (servers == null)
                servers = new List<GoPrtgServer>();

            var encryptedString = EncryptString(client.PassHash);

            var newRecord = new GoPrtgServer(client.Server, Alias, client.UserName, encryptedString);

            servers.Add(newRecord);

            var newContents = new List<string>();

            newContents.Add($"function {goPrtgFunction} {{@(");

            for (var i = 0; i < servers.Count; i++)
            {
                var str = "    " + servers[i];

                if (i < servers.Count - 1)
                    str += ",";

                newContents.Add(str);
            }

            newContents.Add(")}");

            return newContents;
        }

        private void CheckServerAgainstExistingRecords(List<GoPrtgServer> servers)
        {
            var record = servers.FirstOrDefault(s => s.Server == client.Server);

            if (record != null)
            {
                if (record.Alias != null)
                {
                    if (record.UserName == client.UserName)
                        throw new InvalidOperationException($"Cannot add server '{client.Server}': a record for the user '{client.UserName}' already exists. To update the alias of this record use Set-GoPrtgAlias. To reinstall this record, first uninstall with Uninstall-GoPrtgServer and then re-run Install-GoPrtgServer.");

                    if (string.IsNullOrEmpty(Alias))
                        throw new InvalidOperationException($"Cannot add server '{client.Server}': an alias must be specified to differentiate this connection from an existing connection with the same server address.");
                }
                else
                {
                    if (record.UserName == client.UserName)
                        throw new InvalidOperationException($"Cannot add server '{client.Server}': a record for the user '{client.UserName}' already exists.");

                    throw new InvalidOperationException($"Cannot add server '{client.Server}': a record for the server already exists without an alias. Please update the alias of this record with Set-GoPrtgAlias and try again.");
                }
            }
            else
            {
                if (servers.Any(s => s.Alias == Alias) && !string.IsNullOrEmpty(Alias))
                    throw new InvalidOperationException($"Cannot add server '{client.Server}' with alias '{Alias}': a record for the alias already exists. For more information see 'Get-GoPrtgServer {Alias}'.");                 
            }
        }

        private class GoPrtgProfileContents
        {
            public List<string> Pre { get; set; }

            public List<string> Func { get; set; }

            public List<string> Post { get; set; }

            public GoPrtgProfileContents(List<string> pre, List<string> func, List<string> post)
            {
                Pre = pre;
                Func = func;
                Post = post;
            }
        }
    }
}
