using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    class ChannelPropertiesParameters : BaseParameters, IHtmlParameters
    {
        HtmlFunction IHtmlParameters.Function => HtmlFunction.ChannelEdit;

        public ChannelPropertiesParameters(Either<Sensor, int> sensorOrId, int channelId)
        {
            SensorId = sensorOrId.GetId();
            ChannelId = channelId;
        }

        public int SensorId
        {
            get { return (int)this[Parameter.Id]; }
            set { this[Parameter.Id] = value; }
        }

        public int ChannelId
        {
            get { return (int)this[Parameter.Channel]; }
            set { this[Parameter.Channel] = value; }
        }
    }
}
