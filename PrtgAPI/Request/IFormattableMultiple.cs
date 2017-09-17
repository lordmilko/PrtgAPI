namespace PrtgAPI
{
    interface IFormattableMultiple : IFormattable
    {
        string[] GetSerializedFormats();
    }
}
