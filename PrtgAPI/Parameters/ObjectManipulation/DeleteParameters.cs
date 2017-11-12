using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    class DeleteParameters : BaseActionParameters
    {
        public DeleteParameters(int objectId) : base(objectId)
        {
            this[Parameter.Approve] = 1;
        }
    }
}
