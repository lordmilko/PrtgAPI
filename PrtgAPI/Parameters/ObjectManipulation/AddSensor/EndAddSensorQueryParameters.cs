namespace PrtgAPI.Parameters
{
    class EndAddSensorQueryParameters : BaseActionParameters, IHtmlParameters
    {
        HtmlFunction IHtmlParameters.Function => HtmlFunction.AddSensor4;

        public EndAddSensorQueryParameters(int deviceId, int tmpId) : base(deviceId)
        {
            this[Parameter.Custom] = new CustomParameter("tmpid", tmpId);

            //Request relies on cookie in order to authenticate. Respecifying credentials (performed automatically by PrtgRequestMessage when otherwise not specified)
            //will cause request to fail.
            Cookie = true;
        }
    }
}
