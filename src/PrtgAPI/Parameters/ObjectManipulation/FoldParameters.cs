using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    class FoldParameters : BaseActionParameters, ICommandParameters
    {
        CommandFunction ICommandParameters.Function => CommandFunction.FoldObject;

        public FoldParameters(int objectId, bool fold) : base(objectId)
        {
            this[Parameter.Fold] = fold ? 2 : 1;
        }
    }
}
