using System;
using System.Management.Automation;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// Initializes a new instance of a <see cref="PrtgClient"/>. This cmdlet must be called at least once before attempting to use any other cmdlets.
    /// </summary>
    [Cmdlet(VerbsCommunications.Connect, "PrtgServer")]
    public class ConnectPrtgServer : PSCmdlet
    {
        /// <summary>
        /// Specifies the PRTG Server requests will be made against.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "The address of the PRTG Server to connect to. If the server does not use HTTPS, http:// must be specified.")]
        public string Server { get; set; }

        /// <summary>
        /// Specifies the username and password to authenticate with. If <see cref="PassHash"/> is specified, the password will be treated as a PassHash instead.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ValueFromPipeline = true, Position = 2, HelpMessage = "The username and password to authenticate with. If -PassHash is specified, the password will be treated as a PassHash instead.")]
        public PSCredential Credential { get; set; }

        /// <summary>
        /// Forces a <see cref="PrtgClient"/> to be replaced if one already exists.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Forces the sessions PrtgClient to be replaced if one already exists.")]
        public SwitchParameter Force { get; set; }

        /// <summary>
        /// Specifies that the <see cref="Credential"/>'s password contains a PassHash instead of a Password.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Indicates that the password field of -Credential contains a PassHash instead of a Password.")]
        public SwitchParameter PassHash { get; set; }

        /// <summary>
        /// The number of times to retry a request that times out while communicating with PRTG.
        /// </summary>
        [Parameter(Mandatory = false)]
        public int? RetryCount { get; set; }

        /// <summary>
        /// The base delay (in seconds) between retrying a timed out request. Each successive failure of a given request will wait an additional multiple of this value.
        /// </summary>
        [Parameter(Mandatory = false)]
        public int? RetryDelay { get; set; }

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

                if (RetryCount != null)
                    PrtgSessionState.Client.RetryCount = RetryCount.Value;

                if (RetryDelay != null)
                    PrtgSessionState.Client.RetryDelay = RetryDelay.Value;
            }
            else
            {
                throw new Exception($"Already connected to server {PrtgSessionState.Client.Server}. To override please specify -Force");
            }
        }
    }
}
