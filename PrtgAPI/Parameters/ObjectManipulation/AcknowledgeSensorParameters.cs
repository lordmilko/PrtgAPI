namespace PrtgAPI.Parameters
{
    class AcknowledgeSensorParameters : BaseActionParameters
    {
        public AcknowledgeSensorParameters(int objectId, int? duration, string message) : base(objectId)
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
