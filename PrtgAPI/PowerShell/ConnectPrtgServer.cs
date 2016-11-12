using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.PowerShell
{
    [Cmdlet("Connect", "PrtgServer")]
    public class ConnectPrtgServer : PSCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0, HelpMessage = "Server to connect to.")]
        public string Server { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 1, HelpMessage = "Username to authenticate as.")]
        public string Username { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 3, HelpMessage = "Username to authenticate as.")]
        public string Password { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter Force { get; set; }

        protected override void ProcessRecord()
        {
            if (PrtgSessionState.Client == null || Force.IsPresent)
            {
                PrtgSessionState.Client = new PrtgClient(Server, Username, Password);
            }
            else
            {
                throw new Exception($"Already connected to server {PrtgSessionState.Client.Server}. To override please specify -Force");
            }
        }
    }
}
