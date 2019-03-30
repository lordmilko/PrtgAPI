using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    class PauseParameters : BaseMultiActionParameters, ICommandParameters
    {
        CommandFunction ICommandParameters.Function
        {
            get
            {
                if (Duration != null)
                    return CommandFunction.PauseObjectFor;

                return CommandFunction.Pause;
            }
        }

        public PauseParameters(int[] objectIds, PauseAction action = PauseAction.Pause) : base(objectIds)
        {
            PauseAction = action;
        }

        public PauseParameters(int[] objectIds, int? durationMinutes, string pauseMessage) : base(objectIds)
        {
            if (durationMinutes != null)
                Duration = durationMinutes;

            if (pauseMessage != null)
                PauseMessage = pauseMessage;

            if (Duration == null)
                PauseAction = PauseAction.Pause;
        }

        public PauseAction PauseAction
        {
            get { return (PauseAction) this[Parameter.Action]; }
            set { this[Parameter.Action] = (int) value; }
        }

        public int? Duration
        {
            get { return (int?)this[Parameter.Duration]; }
            set { this[Parameter.Duration] = value; }
        }

        public string PauseMessage
        {
            get { return (string)this[Parameter.PauseMessage]; }
            set { this[Parameter.PauseMessage] = value; }
        }
    }
}
