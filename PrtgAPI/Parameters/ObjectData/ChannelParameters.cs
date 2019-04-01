using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    class ChannelParameters : ContentParameters<Channel>
    {
        public ChannelParameters(Either<Sensor, int> sensorOrId) : base(Content.Channels)
        {
            this[Parameter.Id] = sensorOrId.GetId();
        }
    }
}
