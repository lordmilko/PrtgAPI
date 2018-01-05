using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    class PauseParameters : PauseParametersBase
    {
        public PauseParameters(int[] objectIds, PauseAction action = PauseAction.Pause) : base(objectIds)
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
