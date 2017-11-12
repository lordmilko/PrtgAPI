using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    class ChannelParameters : ContentParameters<Channel>
    {
        public ChannelParameters(int sensorId) : base(Content.Channels)
        {
            this[Parameter.Id] = sensorId;
        }
    }
}
