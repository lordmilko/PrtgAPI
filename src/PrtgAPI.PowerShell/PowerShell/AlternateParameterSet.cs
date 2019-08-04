namespace PrtgAPI.PowerShell
{
    class AlternateParameterSet
    {
        public string Name { get; }

        public IAlternateParameter[] Parameters { get; }

        public AlternateParameterSet(string name, params IAlternateParameter[] parameters)
        {
            Name = name;
            Parameters = parameters;
        }
    }
}