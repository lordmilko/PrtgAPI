using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    class PauseForDurationParameters : PauseParametersBase
    {
        public PauseForDurationParameters(int[] objectIds, int duration) : base(objectIds)
        {
            Duration = duration;
        }

        public int Duration
        {
            get { return (int) this[Parameter.Duration]; }
            set { this[Parameter.Duration] = value; }
        }
    }
}
