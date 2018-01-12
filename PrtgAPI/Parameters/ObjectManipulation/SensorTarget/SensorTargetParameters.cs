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
            get { return (SensorType) this[Parameter.SensorType]; }
            set { this[Parameter.SensorType] = value; }
        }
    }
}
