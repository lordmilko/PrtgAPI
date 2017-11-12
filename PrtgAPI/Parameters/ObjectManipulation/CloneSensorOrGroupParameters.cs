using System;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    class CloneSensorOrGroupParameters : BaseActionParameters
    {
        public CloneSensorOrGroupParameters(int objectId, string cloneName, int targetId) : base(objectId)
        {
            if (string.IsNullOrEmpty(cloneName))
                throw new ArgumentException("cloneName cannot be null or empty", nameof(cloneName));

            CloneName = cloneName;
            TargetId = targetId;
        }

        public string CloneName
        {
            get { return (string)this[Parameter.Name]; }
            set { this[Parameter.Name] = value; }
        }

        public int TargetId
        {
            get { return (int)this[Parameter.TargetId]; }
            set { this[Parameter.TargetId] = value; }
        }
    }
}
