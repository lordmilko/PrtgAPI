namespace PrtgAPI.Parameters
{
    class AddSensorProgressParameters : BaseActionParameters, IJsonParameters
    {
        JsonFunction IJsonParameters.Function => JsonFunction.GetAddSensorProgress;

        public AddSensorProgressParameters(Either<Device, int> deviceOrId, int tmpId) : base(deviceOrId.ToPrtgObject())
        {
            this[Parameter.TmpId] = tmpId;

            //Request relies on cookie in order to authenticate. Respecifying credentials (performed automatically by PrtgRequestMessage when otherwise not specified)
            //will cause request to fail.
            Cookie = true;
        }

        public AddSensorProgressParameters(Either<Device, int> deviceOrId, int tmpId, bool isFinalStep) : this(deviceOrId, tmpId)
        {
            if (isFinalStep)
                this[Parameter.Custom] = new CustomParameter("step", 3);
        }
    }
}
