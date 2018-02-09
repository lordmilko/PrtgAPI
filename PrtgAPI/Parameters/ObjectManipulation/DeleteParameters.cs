using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    class DeleteParameters : BaseMultiActionParameters
    {
        public DeleteParameters(int[] objectIds) : base(objectIds)
        {
            this[Parameter.Approve] = 1;
        }
    }
}
