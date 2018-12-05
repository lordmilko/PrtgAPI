namespace PrtgAPI.Parameters
{
    class SimpleParameterContainer : IParameterContainer
    {
        public void Initialize(IParameters parameters)
        {
        }

        public void Add(object value)
        {
        }

        public IParameterContainerValue CreateValue(string name, object value, bool trimName)
        {
            return new SimpleParameterContainerValue(name, value);
        }
    }
}
