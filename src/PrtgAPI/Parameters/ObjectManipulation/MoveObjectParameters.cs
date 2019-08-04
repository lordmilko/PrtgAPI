using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    class MoveObjectParameters : BaseActionParameters, ICommandParameters
    {
        CommandFunction ICommandParameters.Function => CommandFunction.MoveObjectNow;

        public MoveObjectParameters(Either<IPrtgObject, int> objectOrId, Either<GroupOrProbe, int> destination) : base(objectOrId)
        {
            TargetId = destination.GetId();
        }

        public int TargetId
        {
            get { return (int)this[Parameter.TargetId]; }
            set { this[Parameter.TargetId] = value; }
        }
    }
}
