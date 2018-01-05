using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    class AcknowledgeSensorParameters : BaseMultiActionParameters
    {
        public AcknowledgeSensorParameters(int[] objectIds, int? duration, string message) : base(objectIds)
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
