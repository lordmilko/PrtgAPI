using System;
using PrtgAPI.Request.Serialization.ValueConverters;

namespace PrtgAPI.Parameters
{
    class SetPositionParameters : BaseActionParameters, ICommandParameters
    {
        CommandFunction ICommandParameters.Function => CommandFunction.SetPosition;

        public SetPositionParameters(Either<IPrtgObject, int> objectOrId, Position position) : base(objectOrId)
        {
            Position = position;
        }

        public SetPositionParameters(SensorOrDeviceOrGroupOrProbe obj, int position) : base(ValidateObject(obj))
        {
            var newPos = PositionConverter.SerializePosition(position) + (position > obj.Position ? 1 : -1);

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
