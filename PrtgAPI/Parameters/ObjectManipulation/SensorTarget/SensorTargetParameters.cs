using PrtgAPI.Helpers;

namespace PrtgAPI.Parameters
{
    class SensorTargetParameters : BaseActionParameters
    {
        public SensorTargetParameters(int deviceId, SensorType type) : base(deviceId)
        {
            SensorType = type;
        }

        public SensorType SensorType
        {
            get { return this[Parameter.SensorType].ToString().XmlToEnum<SensorType>(); }
            set { this[Parameter.SensorType] = value.EnumToXml().ToLower(); }
        }
    }
}
