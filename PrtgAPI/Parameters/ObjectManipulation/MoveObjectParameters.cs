using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    class MoveObjectParameters : BaseActionParameters
    {
        public MoveObjectParameters(int objectId, int targetId) : base(objectId)
        {
            TargetId = targetId;
        }

        public int TargetId
        {
            get { return (int)this[Parameter.TargetId]; }
            set { this[Parameter.TargetId] = value; }
        }
    }
}
