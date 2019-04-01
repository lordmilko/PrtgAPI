namespace PrtgAPI.Parameters
{
    class SortAlphabeticallyParameters : BaseActionParameters, ICommandParameters
    {
        CommandFunction ICommandParameters.Function => CommandFunction.SortSubObjects;

        public SortAlphabeticallyParameters(Either<IPrtgObject, int> objectOrId) : base(objectOrId)
        {
        }
    }
}
