namespace PrtgAPI.Parameters
{
    class ServerStatusParameters : BaseActionParameters, IJsonParameters
    {
        JsonFunction IJsonParameters.Function => JsonFunction.GetStatus;

        public ServerStatusParameters() : base(0)
        {
        }
    }
}
