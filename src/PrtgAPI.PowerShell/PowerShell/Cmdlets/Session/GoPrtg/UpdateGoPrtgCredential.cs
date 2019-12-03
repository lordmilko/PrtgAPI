using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.PowerShell.GoPrtg;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Updates the credentials used for the currently active GoPrtg server.</para>
    /// 
    /// <para type="description">The Update-GoPrtgCredential cmdlet updates the authentication credentials of the currently
    /// active GoPrtg Server. When your PRTG user account password changes, your PassHash can also change too. In this scenario,
    /// you can simply run the Update-GoPrtgCredential cmdlet to re-enter your credentials. Update-GoPrtgCredential will then resolve
    /// your new PassHash and update the details for the currently active record. Alternatively, you may also update the username
    /// of the currently active record, so long as you do not have another record with the same server/username combination in your
    /// current PowerShell Profile.</para>
    /// 
    /// <example>
    ///     <code>C:\> Update-GoPrtgCredential</code>
    ///     <para>Update the credentials of the currently active GoPrtg server.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Update-GoPrtgCredential (New-Credential prtgadmin prtgadmin)</code>
    ///     <para>Update the credentials of the currently active Goprtg server with a specified username and password.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Store-Credentials#modification">Online version:</para>
    /// <para type="link">Connect-GoPrtgServer</para>
    /// <para type="link">Get-GoPrtgServer</para>
    /// <para type="link">Install-GoPrtgServer</para>
    /// <para type="link">Set-GoPrtgAlias</para>
    /// <para type="link">Uninstall-GoPrtgServer</para>
    /// 
    /// </summary>
    [Cmdlet(VerbsData.Update, "GoPrtgCredential")]
    public sealed class UpdateGoPrtgCredential : GoPrtgCmdlet
    {
        /// <summary>
        /// <para type="description">The new credential to use for the currently active GoPrtg server.</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 0)]
        public PSCredential Credential { get; set; }

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
            UpdateServerRunner(UpdateCredential);

            WriteColorOutput("\nSuccessfully updated credential", ConsoleColor.Green);

            WriteColorOutput($"\nConnected to {client.Server} as {client.UserName}\n", ConsoleColor.Green);
        }

        private Action<GoPrtgServer> UpdateCredential(List<GoPrtgServer> servers, GoPrtgServer activeServer)
        {
            var oldClient = client;
            var newClient = GetNewClient();

            if (oldClient.UserName != newClient.UserName)
            {
                var duplicateServer = servers.Where(s => s.Server == newClient.Server && s.UserName == newClient.UserName).ToList();

                if (duplicateServer.Count > 0)
                    throw new InvalidOperationException($"Cannot update credential: a record with username '{newClient.UserName}' for server '{newClient.Server}' already exists.");
            }

            var encryptedString = EncryptString(newClient.PassHash);

            return server =>
            {
                server.UserName = newClient.UserName;
                server.PassHash = encryptedString;
            };
        }

        private PrtgClient GetNewClient()
        {
            var oldClient = client;

            if (Credential == null)
                Credential = GetNewCredential(oldClient.UserName);

            //Invoke Connect-PrtgServer via PowerShell Invocation instead of just calling the cmdlet directly
            //so we can mock it in Update-GoPrtgCredential's unit tests (otherwise the new PrtgClient's HttpClient
            //would try and connect to a PRTG Server)

            var argsVar = PrtgSessionState.PSEdition == PSEdition.Desktop ? "input" : "args";

            InvokeCommand.InvokeScript($"Connect-PrtgServer -Server ${argsVar}[0] -Credential ${argsVar}[1] -Force", new object[] {oldClient.Server, Credential});

            //Return the PrtgSessionState client
            return client;
        }

        [ExcludeFromCodeCoverage]
        private PSCredential GetNewCredential(string username)
        {
            var credential = Host.UI.PromptForCredential(null, null, username, "targetname");

            if (credential == null)
                throw new ParameterBindingException("Cannot bind argument to parameter 'Credential' because it is null.");

            return credential;
        }
    }
}
