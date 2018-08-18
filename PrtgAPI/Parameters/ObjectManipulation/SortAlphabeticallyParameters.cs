namespace PrtgAPI.Parameters
{
    class SortAlphabeticallyParameters : BaseActionParameters, ICommandParameters
    {
        CommandFunction ICommandParameters.Function => CommandFunction.SortSubObjects;

        public SortAlphabeticallyParameters(int objectId) : base(objectId)
        {
        }
    }
}
