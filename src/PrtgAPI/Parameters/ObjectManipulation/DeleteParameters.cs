using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    class DeleteParameters : BaseMultiActionParameters, ICommandParameters
    {
        CommandFunction ICommandParameters.Function => CommandFunction.DeleteObject;

        public DeleteParameters(int[] objectIds) : base(objectIds)
        {
            this[Parameter.Approve] = 1;
        }
    }
}
