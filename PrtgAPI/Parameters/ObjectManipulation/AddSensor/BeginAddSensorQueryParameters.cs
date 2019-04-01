using System;
using PrtgAPI.Utilities;

namespace PrtgAPI.Parameters
{
    class BeginAddSensorQueryParameters : BaseActionParameters, ICommandParameters
    {
        CommandFunction ICommandParameters.Function => CommandFunction.AddSensor2;

        public BeginAddSensorQueryParameters(Either<Device, int> deviceOrId, SensorType type) : base(deviceOrId.ToPrtgObject())
        {
            SensorType = type;
        }

        internal BeginAddSensorQueryParameters(Either<Device, int> deviceOrId, string sensorType) : base(deviceOrId.ToPrtgObject())
        {
            if (sensorType == null)
                throw new ArgumentNullException(nameof(sensorType), "Sensor type cannot be null.");

            if (string.IsNullOrWhiteSpace(sensorType))
                throw new ArgumentException("Sensor type cannot be null or whitespace.", nameof(sensorType));

            this[Parameter.SensorType] = sensorType;
        }

        public SensorType SensorType
        {
            get { return this[Parameter.SensorType].ToString().XmlToEnum<SensorType>(); }
            set { this[Parameter.SensorType] = value.EnumToXml().ToLower(); }
        }
    }
}
