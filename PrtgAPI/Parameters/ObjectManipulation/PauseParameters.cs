using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    class PauseParameters : PauseParametersBase
    {
        public PauseParameters(int objectId, PauseAction action = PauseAction.Pause) : base(objectId)
        {
            PauseAction = action;
        }

        public PauseAction PauseAction
        {
            get { return (PauseAction) this[Parameter.Action]; }
            set { this[Parameter.Action] = (int) value; }
        }
    }
}
