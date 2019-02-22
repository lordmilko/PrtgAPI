namespace PrtgAPI.Parameters
{
    class SensorTargetProgressParameters : BaseActionParameters, IJsonParameters
    {
        JsonFunction IJsonParameters.Function => JsonFunction.GetAddSensorProgress;

        public SensorTargetProgressParameters(int deviceId, int tmpId) : base(deviceId)
        {
            this[Parameter.Custom] = new CustomParameter("tmpid", tmpId);

            //Request relies on cookie in order to authenticate. Respecifying credentials (performed automatically by PrtgRequestMessage when otherwise not specified)
            //will cause request to fail.
            Cookie = true;
        }
    }
}
