using System;
using System.Management.Automation;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// Initializes a new instance of a <see cref="PrtgClient"/>. This cmdlet must be called at least once before attempting to use any other cmdlets.
    /// </summary>
    [Cmdlet("Connect", "PrtgServer")]
    public class ConnectPrtgServer : PSCmdlet
    {
        /// <summary>
        /// Specifies the PRTG Server requests will be made against.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Server to connect to.")]
        public string Server { get; set; }

        /// <summary>
        /// Specifies the username and password to authenticate with. If <see cref="PassHash"/> is specified, the password will be treated as a PassHash instead.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ValueFromPipeline = true, Position = 2)]
        public PSCredential Credential { get; set; }

        /// <summary>
        /// Forces a <see cref="PrtgClient"/> to be replaced if one already exists.
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Force { get; set; }

        /// <summary>
        /// Specifies that the <see cref="Credential"/>'s password contains a PassHash instead of a Password.
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter PassHash { get; set; }

        /// <summary>
        /// Provides a record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            if (PrtgSessionState.Client == null || Force.IsPresent)
            {
                PrtgSessionState.Client = PassHash.IsPresent ?
                    new PrtgClient(Server, Credential.GetNetworkCredential().UserName, Credential.GetNetworkCredential().Password, AuthMode.PassHash) :
                    new PrtgClient(Server, Credential.GetNetworkCredential().UserName, Credential.GetNetworkCredential().Password);
            }
            else
            {
                throw new Exception($"Already connected to server {PrtgSessionState.Client.Server}. To override please specify -Force");
            }
        }
    }
}
