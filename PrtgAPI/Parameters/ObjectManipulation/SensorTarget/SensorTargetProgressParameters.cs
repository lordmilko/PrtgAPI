namespace PrtgAPI.Parameters
{
    class SensorTargetProgressParameters : BaseActionParameters
    {
        public SensorTargetProgressParameters(int deviceId, int tmpId) : base(deviceId)
        {
            this[Parameter.Custom] = new CustomParameter("tmpid", tmpId);

            //Request relies on cookie in order to authenticate. Respecifying credentials (performed automatically by PrtgUrl when otherwise not specified)
            //will cause request to fail.
            Cookie = true;
        }
    }
}
