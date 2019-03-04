using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    class AcknowledgeSensorParameters : BaseMultiActionParameters, ICommandParameters
    {
        CommandFunction ICommandParameters.Function => CommandFunction.AcknowledgeAlarm;

        public AcknowledgeSensorParameters(int[] sensorIds, int? duration, string message) : base(sensorIds)
        {
            if (message != null)
                Message = message;

            if (duration != null)
                Duration = duration;
        }

        public string Message
        {
            get { return (string)this[Parameter.AcknowledgeMessage]; }
            set { this[Parameter.AcknowledgeMessage] = value; }
        }

        public int? Duration
        {
            get { return (int?)this[Parameter.Duration]; }
            set { this[Parameter.Duration] = value; }
        }
    }
}
