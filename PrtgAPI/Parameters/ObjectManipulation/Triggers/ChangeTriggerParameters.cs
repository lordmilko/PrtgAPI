using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Parameters
{
    public class ChangeTriggerParameters : TriggerParameters
    {
        public ChangeTriggerParameters(int objectId, int? triggerId, ModifyAction action) : base(TriggerType.Change, objectId, triggerId, action)
        {
        }
    }
}
