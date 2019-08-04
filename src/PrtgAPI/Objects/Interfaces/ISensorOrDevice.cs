namespace PrtgAPI
{
    interface ISensorOrDevice : ISensorOrDeviceOrGroup
    {
        string Group { get; }

        bool Favorite { get; }
    }
}
