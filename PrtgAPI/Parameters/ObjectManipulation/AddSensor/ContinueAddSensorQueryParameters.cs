using System.Collections.Generic;
using System.Linq;

namespace PrtgAPI.Parameters
{
    class ContinueAddSensorQueryParameters : BaseActionParameters, ICommandParameters
    {
        CommandFunction ICommandParameters.Function => CommandFunction.AddSensor3;

        public ContinueAddSensorQueryParameters(int deviceId, int tmpId, Dictionary<string, string> parameters) : base(deviceId)
        {
            this[Parameter.TmpId] = tmpId;
            this[Parameter.Custom] = parameters.Select(kv => new CustomParameter(kv.Key, kv.Value)).ToList();

            Cookie = true;
        }
    }
}