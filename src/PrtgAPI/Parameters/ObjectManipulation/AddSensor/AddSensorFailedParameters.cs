namespace PrtgAPI.Parameters
{
    class AddSensorFailedParameters : BaseActionParameters, IHtmlParameters
    {
        HtmlFunction IHtmlParameters.Function => HtmlFunction.AddSensorFailed;

        public AddSensorFailedParameters(Either<Device, int> deviceOrId, string sensorKind) : base(deviceOrId.ToPrtgObject())
        {
            this[Parameter.Custom] = new[]
            {
                new CustomParameter("sensorkind", sensorKind.Replace(" ", "+")),
                new CustomParameter("_hjax", "true")
            };

            Cookie = true;
        }
    }
}