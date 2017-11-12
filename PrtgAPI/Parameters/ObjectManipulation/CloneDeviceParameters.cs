using System;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    class CloneDeviceParameters : CloneSensorOrGroupParameters
    {
        public CloneDeviceParameters(int objectId, string cloneName, int targetId, string host) : base(objectId, cloneName, targetId)
        {
            if (string.IsNullOrEmpty(host))
                throw new ArgumentException("host cannot be null or empty", nameof(host));

            Host = host;
        }

        public string Host
        {
            get { return (string)this[Parameter.Host]; }
            set { this[Parameter.Host] = value; }
        }
    }
}
