namespace PrtgAPI.Parameters
{
    class SimulateErrorParameters : Parameters
    {
        public SimulateErrorParameters(int sensorId)
        {
            SensorId = sensorId;
            Action = 1;
        }

        public int SensorId
        {
            get { return (int)this[Parameter.Id]; }
            set { this[Parameter.Id] = value; }
        }

        public int Action
        {
            get { return (int) this[Parameter.Action]; }
            private set { this[Parameter.Action] = value; }
        }
    }
}
