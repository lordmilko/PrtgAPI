namespace PrtgAPI.Parameters
{
    class AddSensorProgressParameters : BaseActionParameters, IJsonParameters
    {
        JsonFunction IJsonParameters.Function => JsonFunction.GetAddSensorProgress;

        public AddSensorProgressParameters(int deviceId, int tmpId) : base(deviceId)
        {
            this[Parameter.TmpId] = tmpId;

            //Request relies on cookie in order to authenticate. Respecifying credentials (performed automatically by PrtgRequestMessage when otherwise not specified)
            //will cause request to fail.
            Cookie = true;
        }

        public AddSensorProgressParameters(int deviceId, int tmpId, bool isFinalStep) : this(deviceId, tmpId)
        {
            if (isFinalStep)
                this[Parameter.Custom] = new CustomParameter("step", 3);
        }
    }
}
