namespace PrtgAPI.Parameters
{
    interface IParameterContainerValue
    {
        string Name { get; }

        object Value { get; set; }

        void SetValue(object value, bool safe);
    }
}
