using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Parameters
{
    class ModificationHistoryParameters : ContentParameters<ModificationEvent>
    {
        public ModificationHistoryParameters(int objectId) : base(Content.History)
        {
            this[Parameter.Id] = objectId;
        }
    }
}
