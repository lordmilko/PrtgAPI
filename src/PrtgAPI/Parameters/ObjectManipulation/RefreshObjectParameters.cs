namespace PrtgAPI.Parameters
{
    class RefreshObjectParameters : BaseMultiActionParameters, ICommandParameters
    {
        CommandFunction ICommandParameters.Function => CommandFunction.ScanNow;

        public RefreshObjectParameters(int[] objectIds) : base(objectIds)
        {
        }
    }
}
