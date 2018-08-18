namespace PrtgAPI.Parameters
{
    class NotificationTriggerDataParameters : BaseActionParameters, IJsonParameters
    {
        JsonFunction IJsonParameters.Function => JsonFunction.Triggers;

        public NotificationTriggerDataParameters(int objectId) : base(objectId)
        {
        }
    }
}
