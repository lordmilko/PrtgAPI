using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Parameters
{
    class MoveObjectParameters : BaseActionParameters
    {
        public MoveObjectParameters(int objectId, int targetId) : base(objectId)
        {
            TargetId = targetId;
        }

        public int TargetId
        {
            get { return (int)this[Parameter.TargetId]; }
            set { this[Parameter.TargetId] = value; }
        }
    }
}
