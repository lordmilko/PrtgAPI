namespace PrtgAPI.Parameters
{
    class SensorTypeParameters : BaseActionParameters, IJsonParameters
    {
        JsonFunction IJsonParameters.Function => JsonFunction.SensorTypes;

        public SensorTypeParameters(int objectId) : base(objectId)
        {
        }
    }
}
