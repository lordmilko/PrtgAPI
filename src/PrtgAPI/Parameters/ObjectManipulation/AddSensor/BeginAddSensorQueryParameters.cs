using System;
using PrtgAPI.Utilities;

namespace PrtgAPI.Parameters
{
    class BeginAddSensorQueryParameters : BaseActionParameters, ICommandParameters
    {
        CommandFunction ICommandParameters.Function => CommandFunction.AddSensor2;

        internal SensorMultiQueryTargetParameters QueryParameters { get; }

        public BeginAddSensorQueryParameters(Either<Device, int> deviceOrId, SensorType type, ISensorQueryTargetParameters queryParameters) :
            this(deviceOrId, type.EnumToXml(), queryParameters)
        {
        }

        internal BeginAddSensorQueryParameters(Either<Device, int> deviceOrId, string sensorType, ISensorQueryTargetParameters queryParameters) : base(deviceOrId.ToPrtgObject())
        {
            if (sensorType == null)
                throw new ArgumentNullException(nameof(sensorType), "Sensor type cannot be null.");

            if (string.IsNullOrWhiteSpace(sensorType))
                throw new ArgumentException("Sensor type cannot be null or whitespace.", nameof(sensorType));

            OriginalType = sensorType;

            QueryParameters = queryParameters?.ToMultiQueryParameters();

            if (QueryParameters?.QueryTarget != null)
                QueryTarget = QueryParameters.QueryTarget;
            else
                this[Parameter.SensorType] = sensorType;
        }

        public string OriginalType { get; }

        public string SensorType
        {
            get { return this[Parameter.SensorType].ToString(); }
            set { this[Parameter.SensorType] = value.ToString(); }
        }

        public SensorQueryTarget QueryTarget
        {
            get
            {
                return ((CustomParameter)this[Parameter.Custom])?.Value as SensorQueryTarget;
            }
            set
            {
                var sensorType = $"{OriginalType}_nolist";
                this[Parameter.Custom] = new CustomParameter($"preselection_{OriginalType}_nolist", value);
                this[Parameter.SensorType] = sensorType;
            }
        }
    }
}
