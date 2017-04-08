using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Parameters
{
    class BaseActionParameters : Parameters
    {
        public BaseActionParameters(int objectId)
        {
            ObjectId = objectId;
        }

        public int ObjectId
        {
            get { return (int)this[Parameter.Id]; }
            set { this[Parameter.Id] = value; }
        }
    }
}
