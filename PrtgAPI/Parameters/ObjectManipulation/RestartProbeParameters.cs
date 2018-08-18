namespace PrtgAPI.Parameters
{
    class RestartProbeParameters : BaseParameters, ICommandParameters
    {
        CommandFunction ICommandParameters.Function => CommandFunction.RestartProbes;

        public RestartProbeParameters(int? objectId)
        {
            if (objectId != null)
                this[Parameter.Id] = objectId;
        }
    }
}
