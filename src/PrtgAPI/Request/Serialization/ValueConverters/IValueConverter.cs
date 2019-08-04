namespace PrtgAPI.Request.Serialization.ValueConverters
{
    interface IValueConverter
    {
        object Serialize(object value);

        object Deserialize(object value);
    }

    interface IValueConverter<T> : IValueConverter
    {
        string Serialize(T value);

        T Deserialize(T value);
    }
}
