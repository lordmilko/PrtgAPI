namespace PrtgAPI.Request.Serialization.ValueConverters
{
    interface IZeroPaddingConverter : IValueConverter
    {
        string SerializeWithPadding(object value, bool pad);

        string Pad(object value, bool pad);
    }
}
