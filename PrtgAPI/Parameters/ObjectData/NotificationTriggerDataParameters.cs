namespace PrtgAPI.Parameters
{
    class NotificationTriggerDataParameters : BaseActionParameters, IJsonParameters
    {
        JsonFunction IJsonParameters.Function => JsonFunction.Triggers;

        public NotificationTriggerDataParameters(Either<IPrtgObject, int> objectOrId) : base(objectOrId)
        {
        }
    }
}
