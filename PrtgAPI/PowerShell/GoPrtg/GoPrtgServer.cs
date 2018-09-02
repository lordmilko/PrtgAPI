using System.Management.Automation;

namespace PrtgAPI.PowerShell.GoPrtg
{
    class GoPrtgServer
    {
        public bool IsActive { get; set; }

        public string Server { get; set; }

        public string Alias { get; set; }

        public string UserName { get; set; }

        public string PassHash { get; set; }

        public GoPrtgServer(PSObject obj)
        {
            Server = obj.Properties[nameof(Server)].Value?.ToString();
            Alias = obj.Properties[nameof(Alias)].Value?.ToString();

            if (Alias == string.Empty)
                Alias = null;

            UserName = obj.Properties[nameof(UserName)].Value?.ToString();
            PassHash = obj.Properties[nameof(PassHash)].Value?.ToString();
        }

        public GoPrtgServer(string server, string alias, string username, string passhash)
        {
            Server = server;
            Alias = alias;
            UserName = username;
            PassHash = passhash;
        }

        public override string ToString()
        {
            var q = "`\"";

            var alias = string.IsNullOrEmpty(Alias) ? "" :$"{q}{Alias}{q}";

            return $"\"{q}{Server}{q},{alias},{q}{UserName}{q},{q}{PassHash}{q}\"";
        }
    }
}
