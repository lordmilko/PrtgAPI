using System;
using PrtgAPI.Utilities;

namespace PrtgAPI.Parameters
{
    class BeginAddSensorQueryParameters : BaseActionParameters, ICommandParameters
    {
        CommandFunction ICommandParameters.Function => CommandFunction.AddSensor2;

        public BeginAddSensorQueryParameters(int deviceId, SensorType type) : base(deviceId)
        {
            SensorType = type;
        }

        internal BeginAddSensorQueryParameters(int deviceId, string sensorType) : base(deviceId)
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
