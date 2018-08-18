namespace PrtgAPI
{
    interface ISensorOrDevice : ISensorOrDeviceOrGroup
    {
        string Group { get; set; }

        bool Favorite { get; set; }
    }
}
