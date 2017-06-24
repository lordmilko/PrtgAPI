using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Parameters
{
    class ObjectHistoryParameters : ContentParameters<ObjectHistory>
    {
        public ObjectHistoryParameters(int objectId) : base(Content.History)
        {
            this[Parameter.Id] = objectId;
        }
    }
}
