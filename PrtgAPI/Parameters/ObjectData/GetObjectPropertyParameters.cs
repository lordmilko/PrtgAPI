using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Parameters
{
    class GetObjectPropertyParameters : BaseActionParameters
    {
        public GetObjectPropertyParameters(int objectId, string objectType) : base(objectId)
        {
            ObjectType = objectType;
        }

        public string ObjectType
        {
            get { return (string)this[Parameter.ObjectType]; }
            set { this[Parameter.ObjectType] = value; }
        }
    }
}
