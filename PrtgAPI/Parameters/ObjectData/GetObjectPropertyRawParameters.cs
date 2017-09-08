using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Parameters
{
    class GetObjectPropertyRawParameters : BaseActionParameters
    {
        public GetObjectPropertyRawParameters(int objectId, string name) : base(objectId)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Argument must have a value", nameof(name));

            if (name.EndsWith("_"))
                name = name.Substring(0, name.Length - 1);

            Name = name;
        }

        public string Name
        {
            get { return (string)this[Parameter.Name]; }
            set { this[Parameter.Name] = value; }
        }
    }
}
