namespace PrtgAPI.Request
{
    interface IMultipleSerializable : ISerializable
    {
        string[] GetSerializedFormats();
    }
}
