namespace PrtgAPI.Parameters
{
    class ChannelParameters : ContentParameters<Channel>
    {
        public ChannelParameters(int sensorId) : base(Content.Channels)
        {
            this[Parameter.Id] = sensorId;
        }
    }
}
