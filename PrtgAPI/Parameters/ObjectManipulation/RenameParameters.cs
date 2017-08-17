using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Parameters
{
    class RenameParameters : BaseActionParameters
    {
        public RenameParameters(int objectId, string name) : base(objectId)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            Name = name;
        }

        public string Name
        {
            get { return (string)this[Parameter.Name]; }
            set { this[Parameter.Name] = value; }
        }
    }
}
