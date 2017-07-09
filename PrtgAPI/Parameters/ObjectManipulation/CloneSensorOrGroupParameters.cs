using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Parameters
{
    class CloneSensorOrGroupParameters : BaseActionParameters
    {
        public CloneSensorOrGroupParameters(int objectId, string cloneName, int targetId) : base(objectId)
        {
            if (cloneName == null)
                throw new ArgumentNullException(nameof(cloneName));

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
