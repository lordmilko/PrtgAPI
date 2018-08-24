namespace PrtgAPI.Request
{
    interface IFormattableMultiple : IFormattable
    {
        string[] GetSerializedFormats();
    }
}
