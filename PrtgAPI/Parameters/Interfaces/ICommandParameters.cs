namespace PrtgAPI.Parameters
{
    interface ICommandParameters : IParameters
    {
        CommandFunction Function { get; }
    }
}
