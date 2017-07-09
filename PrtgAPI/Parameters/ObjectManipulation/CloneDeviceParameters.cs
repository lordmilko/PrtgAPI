using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Parameters
{
    class CloneDeviceParameters : CloneSensorOrGroupParameters
    {
        public CloneDeviceParameters(int objectId, string cloneName, int targetId, string host) : base(objectId, cloneName, targetId)
        {
            if (host == null)
                throw new ArgumentNullException(nameof(host));

            Host = host;
        }

        public string Host
        {
            get { return (string)this[Parameter.Host]; }
            set { this[Parameter.Host] = value; }
        }
    }
}
