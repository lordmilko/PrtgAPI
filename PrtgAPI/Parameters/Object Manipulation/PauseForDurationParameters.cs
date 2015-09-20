namespace Prtg.Parameters
{
    class PauseForDurationParameters : PauseParametersBase
    {
        public PauseForDurationParameters(int objectId, int duration) : base(objectId)
        {
            Duration = duration;
        }

        public int Duration
        {
            get { return (int) this[Parameter.PauseDuration]; }
            set { this[Parameter.PauseDuration] = value; }
        }
    }
}
