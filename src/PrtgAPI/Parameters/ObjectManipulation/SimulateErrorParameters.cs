using System;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    class SimulateErrorParameters : BaseParameters, ICommandParameters
    {
        CommandFunction ICommandParameters.Function => CommandFunction.Simulate;

        public SimulateErrorParameters(int[] sensorIds)
        {
            if (sensorIds == null)
                throw new ArgumentNullException(nameof(sensorIds));

            if (sensorIds.Length == 0)
                throw new ArgumentException("At least one Object ID must be specified.", nameof(sensorIds));

            SensorIds = sensorIds;
            Action = 1;
        }

        public int[] SensorIds
        {
            get { return (int[])this[Parameter.Id]; }
            set { this[Parameter.Id] = value; }
        }

        public int Action
        {
            get { return (int) this[Parameter.Action]; }
            private set { this[Parameter.Action] = value; }
        }
    }
}
