using PrtgAPI.Utilities;

namespace PrtgAPI.Parameters
{
    class SensorTargetParameters : BaseActionParameters, ICommandParameters
    {
        CommandFunction ICommandParameters.Function => CommandFunction.AddSensor2;

        public SensorTargetParameters(int deviceId, SensorType type) : base(deviceId)
        {
            SensorType = type;
        }

        internal SensorTargetParameters(int deviceId, string sensorType) : base(deviceId)
        {
            this[Parameter.SensorType] = sensorType;
        }

        public SensorType SensorType
        {
            get { return this[Parameter.SensorType].ToString().XmlToEnum<SensorType>(); }
            set { this[Parameter.SensorType] = value.EnumToXml().ToLower(); }
        }
    }
}
