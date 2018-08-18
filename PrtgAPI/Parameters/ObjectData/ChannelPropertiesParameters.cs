using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    class ChannelPropertiesParameters : BaseParameters, IHtmlParameters
    {
        HtmlFunction IHtmlParameters.Function => HtmlFunction.ChannelEdit;

        public ChannelPropertiesParameters(int sensorId, int channelId)
        {
            SensorId = sensorId;
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
