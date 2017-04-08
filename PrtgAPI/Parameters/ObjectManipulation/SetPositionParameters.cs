using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.Objects.Shared;

namespace PrtgAPI.Parameters
{
    class SetPositionParameters : BaseActionParameters
    {
        public SetPositionParameters(int objectId, Position position) : base(objectId)
        {
            Position = position;
        }

        public SetPositionParameters(SensorOrDeviceOrGroupOrProbe obj, int position) : base(ValidateObject(obj))
        {
            var newPos = position * 10 + (position > obj.Position ? 1 : -1);

            Position = newPos;
        }

        private static int ValidateObject(SensorOrDeviceOrGroupOrProbe obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return obj.Id;
        }

        public object Position
        {
            get { return this[Parameter.NewPos]; }
            set { this[Parameter.NewPos] = value; }
        }
    }
}
