namespace PrtgAPI.Parameters
{
    interface IParameterContainer
    {
        void Initialize(IParameters parameters);

        void Add(object value);

        IParameterContainerValue CreateValue(string name, object value, bool trimName);
    }
}
