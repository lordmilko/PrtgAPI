using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Parameters
{
    class GetObjectPropertyParameters : BaseActionParameters
    {
        public GetObjectPropertyParameters(int objectId, ObjectType objectType) : base(objectId)
        {
            ObjectType = objectType;
        }

        public ObjectType ObjectType
        {
            get { return (ObjectType)this[Parameter.ObjectType]; }
            set { this[Parameter.ObjectType] = value; }
        }
    }
}
