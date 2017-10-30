using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Request
{
    class ConnectionDetails
    {
        public ConnectionDetails(string server, string username, string passhash)
        {
            Server = server;
            UserName = username;
            PassHash = passhash;
        }

        public string Server { get; internal set; }

        public string UserName { get; internal set; }

        public string PassHash { get; internal set; }
    }
}
